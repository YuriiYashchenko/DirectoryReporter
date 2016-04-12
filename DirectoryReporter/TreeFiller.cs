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

        private static IEnumerable<TreeNode> FlattenTree(TreeView tv)
        {
            return FlattenTree(tv.Nodes);
        }

        private static IEnumerable<TreeNode> FlattenTree(TreeNodeCollection coll)
        {
            return coll.Cast<TreeNode>()
                        .Concat(coll.Cast<TreeNode>()
                                    .SelectMany(x => FlattenTree(x.Nodes)));
        }

        private void Write(FileSystemInfo fileSystemEntity)
        {
            TreeNode parent = new TreeNode(fileSystemEntity.Name);
            parent.Tag = fileSystemEntity.FullName;
            if (TargetTree.Nodes.Count == 0)
            {
                //do something with firest node
                TargetTree.Invoke(new Action<TreeNode>((e) => TargetTree.Nodes.Add(parent)), parent);
                return;
            }
            else {
                if (fileSystemEntity.GetType() == typeof(FileInfo))
                {
                    parent = FlattenTree(TargetTree)
                                            .FirstOrDefault(r => r.Tag.ToString() == ((FileInfo)fileSystemEntity).DirectoryName);
                }
                if (fileSystemEntity.GetType() == typeof(DirectoryInfo))
                {
                    parent = FlattenTree(TargetTree)
                                            .FirstOrDefault(r => r.Tag.ToString() == ((DirectoryInfo)fileSystemEntity).Parent.FullName);
                }
            }
            var currentNode = new TreeNode(fileSystemEntity.Name);
            currentNode.Tag = fileSystemEntity.FullName;
            TargetTree.Invoke(new Action<TreeNode>((e) => parent.Nodes.Add(currentNode)), currentNode);

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
