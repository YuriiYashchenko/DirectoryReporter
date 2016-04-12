using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace DirectoryReporter
{
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
                GetNode(path).Add(
                    new XElement("File",
                        new XAttribute("Name", file.Name),
                        new XAttribute("FullName", file.FullName),
                        new XAttribute("SizeInBytes", file.Length.ToString()),
                        new XAttribute("Created", file.CreationTime.ToString()))
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
                GetNode(path).Add(
                new XElement("Directory",
                    new XAttribute("Name", dir.Name),
                    new XAttribute("FullName", dir.FullName),
                    new XAttribute("Created", dir.CreationTime.ToString()))
                );
                Document.Save(SaveLocation);
            }           
        }

        private XElement GetNode(string path)
        {
            XElement element = Document.Root;
            if (String.IsNullOrEmpty(Storage.RootPath))
            {
                //Logic when directory is drive root
                List<string> ParentPathParts = path.Split(Path.DirectorySeparatorChar).Skip(1).ToList();
                if (ParentPathParts.FirstOrDefault() == "")
                {
                    return element;
                }
                foreach (var part in ParentPathParts)
                {
                    element = element.Elements().First(e => e.Attribute("Name")?.Value == part);
                }
            }
            else
            {
                if (path == Storage.RootPath)
                {
                    return element;
                }
                List<string> ParentPathParts = path.Substring(Storage.RootPath.Length + 1).Split(Path.DirectorySeparatorChar).ToList();
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
