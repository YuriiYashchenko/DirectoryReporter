using System;
using System.Threading;
using System.Windows.Forms;

namespace DirectoryReporter
{
    public partial class MainForm : Form
    {
        private FolderAnalyzer Analyzer;
        private XmlFileWriter WriterToXml;
        private object WriterToUI;
        //private EventWaitHandle OnPathRecived;

        public MainForm()
        {
            InitializeComponent();
            Analyzer = FolderAnalyzer.Instance;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            if (!String.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
            {
                Analyzer.TargetDirectoryPath = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var storage = new DirectoryStorage();
            //storage.OnPathRecived = OnPathRecived;

            Analyzer.Storage = storage;
            Analyzer.OnPathPostingStart += PrepareXmlFileWrite;
            Analyzer.OnPathPostingFinish += FolderScaningFinish;

            string path = @"C:\Program Files\Google";
            Analyzer.StartAnalyze(path);

        }

        private void FolderScaningFinish(object state, EventArgs e)
        {
            MessageBox.Show("Folder scanining has done");
            
        }

        private void XMLPopulatingFinish(object state, EventArgs e)
        {
            MessageBox.Show("Xml file writing has done");

        }

        private void PrepareXmlFileWrite(object state, EventArgs e)
        {
            var writer = new XmlFileWriter(@"C:\Users\Iurii\Desktop\1\1.xml");
            writer.Storage = Analyzer.Storage;
            writer.OnXmlFilePopulateFinish += XMLPopulatingFinish;
            writer.InitialDocumentWriting();
        }
    }
}
