using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace DirectoryReporter
{
    /// <summary>
    /// Consume directory and files from DirectoryStorage.
    /// </summary>
    public class XmlFileWriter
    {
        private XDocument Document;
        private string SaveLocation;

        public delegate void XmlPopulating(Object s, EventArgs e);
        public event XmlPopulating OnXmlFilePopulateStart;
        public event XmlPopulating OnXmlFilePopulateFinish;

        public DirectoryStorage Storage;

        public XmlFileWriter(string xmlFilePath)
        {
            Document = new XDocument();
            Document.Add(new XElement("FileSystemEntities"));
            SaveLocation = xmlFilePath;
            if (File.Exists(SaveLocation))
            {
                File.Delete(xmlFilePath);
            }
        }

        private void Write(FileSystemInfo fileSystemEntity)
        {
            XElement root = Document.Root;
            if (!File.Exists(SaveLocation))
            {
                root = new XElement("FileSystemEntities");
                root.Save(SaveLocation);
            }

            if (fileSystemEntity.GetType() == typeof(FileInfo))
            {
                FileInfo file = (FileInfo)fileSystemEntity;
                string path = file.DirectoryName;
                var rights = String.Empty;
                var owner = String.Empty;
                try
                {
                    string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    owner = (file.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).ToString());
                    var t = file.GetAccessControl()
                        .GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount))
                        .Cast<AuthorizationRule>().FirstOrDefault(r => r.IdentityReference.Value == userName);                    
                    if (t != null)
                    {
                        rights = ((FileSystemAccessRule)t).FileSystemRights.ToString();
                    }
                }
                catch (Exception ex) {
                    /* Honestly, I think that it is not the right way to handle security permissions and owner info.
                       This solution does not consider cases of complex access rules such as user groups and domain accounts. 
                    */
                    int tti = 0;
                }
                GetNode(path).Add(
                    new XElement("File",
                        new XAttribute("Name", file.Name),
                        new XAttribute("SizeInBytes", file.Length.ToString()),
                        new XAttribute("Created", file.CreationTime.ToString()),
                        new XAttribute("Owner", owner),
                        new XAttribute("Security", rights),
                        new XAttribute("FullName", file.FullName)
                        )
                );
            }
            if (fileSystemEntity.GetType() == typeof(DirectoryInfo))
            {

                DirectoryInfo dir = (DirectoryInfo)fileSystemEntity;
                string path = dir.FullName;
                if (dir.Parent != null)
                {
                    path = dir.Parent.FullName;
                }

                var rights = String.Empty;
                var owner = String.Empty;
                try
                {
                    string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    owner = (dir.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).ToString());
                    var t = dir.GetAccessControl()
                        .GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount))
                        .Cast<AuthorizationRule>().FirstOrDefault(r => r.IdentityReference.Value == userName);
                    if (t != null)
                    {
                        rights = ((FileSystemAccessRule)t).FileSystemRights.ToString();
                    }
                }
                catch (Exception ex)
                {
                    int tti = 0;
                }

                GetNode(path).Add(
                new XElement("Directory",
                    new XAttribute("Name", dir.Name),
                    new XAttribute("Owner", owner),
                    new XAttribute("Created", dir.CreationTime.ToString()),
                    new XAttribute("FullName", dir.FullName)
                    )

                );               
            }
        }

        private XElement GetNode(string path)
        {
            XElement element = Document.Root;
            if (String.IsNullOrEmpty(Storage.RootPath))
            {
                //Logic when directory is drive root
                if (element.Nodes().Count() == 0)
                {
                    return element;
                }
                else {
                    element = element.Elements().First();
                    List<string> ParentPathParts = path.Split(Path.DirectorySeparatorChar).Skip(2).ToList();
                    foreach (var part in ParentPathParts)
                    {
                        element = element.Elements().First(e => e.Attribute("Name")?.Value == part);
                    }
                }
            }
            else
            {
                if (path == Storage.RootPath)
                {
                    return element;
                }
                List<string> ParentPathParts = path.Substring(Storage.RootPath.Length).Split(Path.DirectorySeparatorChar).Skip(1).ToList();
                foreach (var part in ParentPathParts)
                {
                    element = element.Elements().First(e => e.Attribute("Name")?.Value == part); // ?. is cool
                }
            }
            return element;
        }

        private void DataReciever()
        {
            try
            {
                bool isNewPathRecived = true;
                while (isNewPathRecived)
                {
                    FileInfoFrame fileSystemEntity = Storage.GetNextXmlPath();
                    if (fileSystemEntity != null)
                    {
                        if (!fileSystemEntity.IsFrameEmpty)
                        {
                            Write(fileSystemEntity.FileSystemEntity);
                        }
                        isNewPathRecived = Storage.OnPathRecived.WaitOne();
                        if (DateTime.Now.Second % 10 == 0)
                        {
                            Document.Save(SaveLocation);
                        }
                    }
                    else {
                        isNewPathRecived = false;
                    }
                }
            }
            catch (Exception ex)
            {
                int tti = 1;
            }
            finally
            {
                Document.Save(SaveLocation);
                if (OnXmlFilePopulateFinish != null)
                {
                    OnXmlFilePopulateFinish(this, EventArgs.Empty);
                }
            }
        }

        public void InitialWriting()
        {
            Thread XmlWriteThread = new Thread(() => DataReciever()) { Name = "XML fill thread" };
            XmlWriteThread.Start();
        }


        // That was temporary solution that not use WaitHandle.
        //private void DataReciever()
        //{
        //    while (true)
        //    {
        //        string path = Storage.GetNextPath();
        //        if (!String.IsNullOrEmpty(path))
        //        {
        //            Write(path);
        //        }
        //        Thread.Sleep(100);
        //    }
        //}
    }
}
