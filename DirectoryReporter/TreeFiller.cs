using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirectoryReporter
{
    public class TreeFiller
    {
        public delegate void TreePopulating(Object s, EventArgs e);
        public event TreePopulating OnTreePopulatingStart;
        public event TreePopulating OnTreePopulatingFinish;

        //public Thread TreeFillThread;

        public DirectoryStorage Storage;

        private TreeView TargetTree;

        public TreeFiller(TreeView tree)
        {
            TargetTree = tree;
        }

        private void Write(FileSystemInfo fileSystemEntity)
        {
            TargetTree.Invoke(new Action<FileSystemInfo>((a) => TargetTree.Nodes.Add(a.Name)), fileSystemEntity);
        }
        private void DataReciever()
        {
            try
            {
                bool isNewPathRecived = true;
                while (isNewPathRecived)
                {
                    FileInfoFrame fileSystemEntity = Storage.GetNextTreePath();
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
                if (OnTreePopulatingFinish != null)
                {
                    OnTreePopulatingFinish(this, EventArgs.Empty);
                }
            }
        }

        public void InitialWriting()
        {
            Thread TreeFillThread = new Thread(() => DataReciever());
            TreeFillThread.Start();
        }
    }
}
