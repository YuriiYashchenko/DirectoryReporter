using System;
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
            Analyzer.OnPathPostingStart += PrepareTreeView;
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

        private void TreePopulatinFinish(object state, EventArgs e)
        {
            MessageBox.Show("Xml file writing has done");
        }

        private void PrepareXmlFileWrite(object state, EventArgs e)
        {
            var writer = new XmlFileWriter(@"C:\Users\Iurii\Desktop\1\1.xml");
            writer.Storage = Analyzer.Storage;
            writer.OnXmlFilePopulateFinish += XMLPopulatingFinish;
            writer.InitialWriting();
        }
        private void PrepareTreeView(object state, EventArgs e)
        {
            var treeFiller = new TreeFiller(this.treeView1);
            treeFiller.Storage = Analyzer.Storage;
            treeFiller.OnTreePopulatingFinish += TreePopulatinFinish;
            treeFiller.InitialWriting();
        }

    }
}
