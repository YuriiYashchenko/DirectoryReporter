using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DirectoryReporter
{
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

        public string GetRootPath() {
            if (FileSystemEntities.Count > 0) {
                return ((DirectoryInfo)FileSystemEntities.First()).Parent.FullName;
            }
            return String.Empty;
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
            Monitor.Enter(this.FileSystemEntities);
            FileSystemEntities.Add(new DirectoryInfo(path));
            Monitor.Exit(this.FileSystemEntities);
            OnPathRecived.Set();
        }

        public void PushFilePath(string path)
        {
            Monitor.Enter(this.FileSystemEntities);
            FileSystemEntities.Add(new FileInfo(path));
            Monitor.Exit(this.FileSystemEntities);
            OnPathRecived.Set();
        }

        //public FileInfoFrame GetParent(FileInfoFrame frame) {
        //    if (frame.FileSystemEntity == FileSystemEntities[0]) {

        //    }
        //}
    }
}
