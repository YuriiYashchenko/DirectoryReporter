using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DirectoryReporter
{
    /*
        This class is something close to provider in Observer pattern
    */
    public class DirectoryStorage
    {
        public EventWaitHandle OnPathRecived;

        private List<FileSystemInfo> FileSystemEntities;

        private volatile bool IsPathReceivingCompleted;

        public void PathReceivingCompleted()
        {
            IsPathReceivingCompleted = true;
            OnPathRecived.Set();
        }

        private int LastXmlSentIndex;

        private int LastTreeSentIndex;

        public string RootPath
        {
            private set;
            get;
        }

        public DirectoryStorage()
        {
            OnPathRecived = new EventWaitHandle(false, EventResetMode.AutoReset);
            FileSystemEntities = new List<FileSystemInfo>();
        }

        public FileInfoFrame GetNextXmlPath()
        {
            var result = new FileInfoFrame();
            if (FileSystemEntities.Count > LastXmlSentIndex)
            {
                OnPathRecived.Set();
                result.FileSystemEntity = FileSystemEntities[LastXmlSentIndex];
                Interlocked.Increment(ref LastXmlSentIndex);
                return result;
            }
            else {
                if (IsPathReceivingCompleted)
                {
                    OnPathRecived.Set();
                    return null;
                }
            }
            result.IsFrameEmpty = true;
            return result;
        }

        public FileInfoFrame GetNextTreePath()
        {
            var result = new FileInfoFrame();
            if (FileSystemEntities.Count > LastTreeSentIndex)
            {
                OnPathRecived.Set();
                result.FileSystemEntity = FileSystemEntities[LastTreeSentIndex];
                Interlocked.Increment(ref LastTreeSentIndex);
                return result;
            }
            else {
                if (IsPathReceivingCompleted)
                {
                    OnPathRecived.Set();
                    return null;
                }
            }
            result.IsFrameEmpty = true;
            return result;
        }


        public void PushDirectoryPath(string path)
        {
            if (path.Length < 260)
            {
                Monitor.Enter(this.FileSystemEntities);
                var dir = new DirectoryInfo(path);
                if (FileSystemEntities.Count == 0)
                {
                    if (dir.FullName == dir.Root.FullName)
                    {
                        RootPath = String.Empty;
                    }
                    else
                    {
                        RootPath = dir.Parent.FullName;
                    }
                }
                FileSystemEntities.Add(dir);
                Monitor.Exit(this.FileSystemEntities);
                OnPathRecived.Set();
            }
        }

        public void PushFilePath(string path)
        {
            if (path.Length < 260)
            {
                Monitor.Enter(this.FileSystemEntities);
                FileSystemEntities.Add(new FileInfo(path));
                Monitor.Exit(this.FileSystemEntities);
                OnPathRecived.Set();
            }
        }

    }
}
