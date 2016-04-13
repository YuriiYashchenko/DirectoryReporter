using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace DirectoryReporter
{
    /// <summary>
    /// Consume directory and files from DirectoryStorage.
    /// </summary>
    public class TreeFiller
    {
        public delegate void TreePopulating(Object s, EventArgs e);
        public event TreePopulating OnTreePopulatingStart;
        public event TreePopulating OnTreePopulatingFinish;

        public DirectoryStorage Storage;

        private Dictionary<string,TreeNode> TreeCache;

        private TreeView TargetTree;

        public TreeFiller(TreeView tree)
        {
            TargetTree = tree;
            TreeCache = new Dictionary<string, TreeNode>();
        }

        private void Write(FileSystemInfo fileSystemEntity)
        {
            TreeNode current = new TreeNode(fileSystemEntity.Name);
            if (TargetTree.Nodes.Count == 0)
            {
                /*  Do something with the first node. Actually nodes are not adding in this thread because modification of WinForm controls
                    is not possible from another thread.
                 */
                TargetTree.Invoke(new Action<TreeNode>((e) => TargetTree.Nodes.Add(current)), current);
                TreeCache.Add(((DirectoryInfo)fileSystemEntity).FullName,current);
                return;
            }
            else {
                /*The rest of nodes are adding using recursive search of in the Tree View nodes.*/
                string parent = String.Empty;
                if (fileSystemEntity.GetType() == typeof(FileInfo))
                {
                    parent = ((FileInfo)fileSystemEntity).DirectoryName;
                }
                if (fileSystemEntity.GetType() == typeof(DirectoryInfo))
                {
                    parent = ((DirectoryInfo)fileSystemEntity).Parent.FullName;
                    TreeCache.Add(((DirectoryInfo)fileSystemEntity).FullName, current);
                }
                TreeNode parentNode;
                if (TreeCache.TryGetValue(parent, out parentNode)) // Returns true.
                {
                    TargetTree.Invoke(new Action<TreeNode>((e) => parentNode.Nodes.Add(current)), current);
                }
            } 
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
            Thread TreeFillThread = new Thread(() => DataReciever()) { Name = "Tree Fill Thread" };
            TreeFillThread.Start();
        }
    }
}
