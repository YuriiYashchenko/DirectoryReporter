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
            Analyzer.Storage = storage;
            Analyzer.OnPathPostingStart += PrepareXmlFileWrite;
            Analyzer.OnPathPostingFinish += ThreadsStatusCheck;
          
            string path = @"C:\Program Files\Git";
            Analyzer.StartAnalyze(path);

        }

        private void ThreadsStatusCheck(object state, EventArgs e)
        {
            MessageBox.Show("Done");
        }
        private void PrepareXmlFileWrite(object state, EventArgs e)
        {
            var writer = new XmlFileWriter(@"C:\Users\Iurii\Desktop\1\1.xml");
            writer.Storage = Analyzer.Storage;
            writer.InitialDocumentWriting();
        }
    }
}
