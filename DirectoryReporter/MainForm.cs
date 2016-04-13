using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace DirectoryReporter
{
    public partial class MainForm : Form
    {
        private FolderAnalyzer Analyzer;

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
            Analyzer.StartAnalyze();
            button2.Enabled = false;

        }

        private void FolderScaningFinish(object state, EventArgs e)
        {
            MessageBox.Show("Folder scanining has done");
        }

        private void XMLPopulatingFinish(object state, EventArgs e)
        {
            MessageBox.Show("Xml file writing has done");
        }

        private void TreePopulatinFinish(object state, EventArgs e)
        {
            MessageBox.Show("Tree writing has done");
        }

        private void PrepareXmlFileWrite(object state, EventArgs e)
        {
            var writer = new XmlFileWriter(@"1.xml");
            writer.Storage = Analyzer.Storage;
            writer.OnXmlFilePopulateFinish += XMLPopulatingFinish;
            writer.InitialWriting();
        }
        private void PrepareTreeView(object state, EventArgs e)
        {
            treeView1.Nodes.Clear();
            var treeFiller = new TreeFiller(this.treeView1);
            treeFiller.Storage = Analyzer.Storage;
            treeFiller.OnTreePopulatingFinish += TreePopulatinFinish;
            treeFiller.InitialWriting();
        }

    }
}
