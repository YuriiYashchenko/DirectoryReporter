using System;
using System.Collections.Generic;
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
        private string SaveLoation;

        public Thread XmlWriteThread;

        public DirectoryStorage Storage;

        public XmlFileWriter(string xmlFilePath) {
            Document = new XDocument();
            Document.Add(new XElement("Root"));
            SaveLoation = xmlFilePath;            
        }
        
        private void Write(string message)
        {
            Monitor.Enter(Document);
            Document.Descendants("Root").FirstOrDefault().Add(new XElement("Message", new XAttribute("path", message)));
            Document.Save(SaveLoation);
            Monitor.Exit(Document);
        }
        private void DataReciever()
        {
            while (true) {
                string path = Storage.GetNextPath();
                if (!String.IsNullOrEmpty(path)) {
                    Write(path);
                }
                Thread.Sleep(100);                                    
            }
        }

        public void InitialDocumentWriting() {           
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
