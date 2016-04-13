using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace DirectoryReporter
{
    public partial class MainForm : Form
    {
        private FolderAnalyzer Analyzer;
        private string XmlSaveFolder;
        public MainForm()
        {
            InitializeComponent();
            Analyzer = FolderAnalyzer.Instance;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            processingFolderBrowserDialog.ShowDialog();
            if (!String.IsNullOrEmpty(processingFolderBrowserDialog.SelectedPath))
            {
                if (Directory.Exists(processingFolderBrowserDialog.SelectedPath)){
                    Analyzer.TargetDirectoryPath = processingFolderBrowserDialog.SelectedPath;
                    button2.Enabled = true;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var storage = new DirectoryStorage();

            Analyzer.Storage = storage;
            Analyzer.OnPathPostingStart += PrepareXmlFileWrite;
            Analyzer.OnPathPostingStart += PrepareTreeView;
            Analyzer.OnPathPostingFinish += FolderScaningFinish;
            Analyzer.OnPathPostingWarning += FolderScaningWarning;
            Analyzer.StartAnalyze();
            button2.Enabled = false;

        }

        private void FolderScaningWarning(object state, TextEventArgs e)
        {
            Analyzer.OnPathPostingWarning -= FolderScaningWarning;
            MessageBox.Show(e.Text);
        }
        
        private void FolderScaningFinish(object state, EventArgs e)
        {
            Analyzer.OnPathPostingFinish -= FolderScaningFinish;
            MessageBox.Show("Folder scanning has done");
        }

        private void XMLPopulatingFinish(object state, EventArgs e)
        {
            Analyzer.OnPathPostingStart -= PrepareXmlFileWrite;
            MessageBox.Show("XML file writing has done");
        }

        private void TreePopulatinFinish(object state, EventArgs e)
        {
            Analyzer.OnPathPostingStart -= PrepareTreeView;
            MessageBox.Show("Tree writing has done");
        }

        private void PrepareXmlFileWrite(object state, EventArgs e)
        {
            string xmlSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"1.xml");
            if (!String.IsNullOrEmpty(XmlSaveFolder))
            {
                xmlSavePath = Path.Combine(XmlSaveFolder, @"1.xml");
            }
            var writer = new XmlFileWriter(xmlSavePath);
            writer.Storage = Analyzer.Storage;
            writer.OnXmlFilePopulateFinish += XMLPopulatingFinish;
            writer.InitialWriting();

            Analyzer.OnPathPostingStart -= PrepareXmlFileWrite;
        }
        private void PrepareTreeView(object state, EventArgs e)
        {
            treeView1.Nodes.Clear();
            var treeFiller = new TreeFiller(this.treeView1);
            treeFiller.Storage = Analyzer.Storage;
            treeFiller.OnTreePopulatingFinish += TreePopulatinFinish;
            treeFiller.InitialWriting();

            Analyzer.OnPathPostingStart -= PrepareTreeView;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            xmlSavingBrowserDialog.ShowDialog();
            if (!String.IsNullOrEmpty(xmlSavingBrowserDialog.SelectedPath))
            {
                if (Directory.Exists(xmlSavingBrowserDialog.SelectedPath))
                {
                    XmlSaveFolder = xmlSavingBrowserDialog.SelectedPath;                    
                }
            }
        }
    }
}
