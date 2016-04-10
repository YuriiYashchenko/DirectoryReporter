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

        public delegate void XmlPopulating(Object s, EventArgs e);
        public event XmlPopulating OnXmlFilePopulateStart;
        public event XmlPopulating OnXmlFilePopulateFinish;

        public Thread XmlWriteThread;

        public DirectoryStorage Storage;

        public XmlFileWriter(string xmlFilePath)
        {
            Document = new XDocument();
            Document.Add(new XElement("Root"));
            SaveLoation = xmlFilePath;
        }

        private void Write(string message)
        {
            if (message != String.Empty)
            {
                Document.Descendants("Root").FirstOrDefault().Add(new XElement("Message", new XAttribute("path", message)));
            }
        }
        private void DataReciever()
        {
            bool isNewPathRecived = true;
            while (isNewPathRecived)
            {
                string path = Storage.GetNextPath();
                if (path != null)
                {
                    Write(path);
                    isNewPathRecived = Storage.OnPathRecived.WaitOne();
                }
                else {
                    isNewPathRecived = false;
                }
            }
            try
            {
                Document.Save(SaveLoation);
            }
            catch (Exception ex)
            {
                int tti = 1;
            }
            if (OnXmlFilePopulateFinish != null)
            {
                OnXmlFilePopulateFinish(this, EventArgs.Empty);
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
