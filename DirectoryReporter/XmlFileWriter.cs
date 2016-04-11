using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public Thread XmlWriteThread;

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
            XElement root = Document.Root; ;
            if (!File.Exists(SaveLocation))
            {
                root = new XElement("FileSystemEntities");
                root.Save(SaveLocation);
            }

            if (fileSystemEntity is FileInfo)
            {
                FileInfo file = (FileInfo) fileSystemEntity;               
                root.Add(
                    new XElement("File",
                         new XElement("FullName", file.FullName),
                         new XElement("SizeInBytes", file.Length.ToString()),
                         new XElement("Created", file.CreationTime.ToString())
                        )
                );
            }
            if (fileSystemEntity is DirectoryInfo)
            {
                DirectoryInfo dir = (DirectoryInfo)fileSystemEntity;                
                root.Add(
                    new XElement("Directory",
                         new XElement("FullName", dir.FullName),
                         //new XElement("SizeInBytes", dir.Length.ToString()),
                         new XElement("Created", dir.CreationTime.ToString())
                        )
                );
            }
        }
        private void DataReciever()
        {
            try
            {
                bool isNewPathRecived = true;
                while (isNewPathRecived)
                {
                    FileInfoFrame fileSystemEntity = Storage.GetNextPath();
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

        public void InitialDocumentWriting()
        {
            Thread XmlWriteThread = new Thread(() => DataReciever());
            XmlWriteThread.Start();
        }


        // That was temporary solution thar not use WaitHandle.
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
