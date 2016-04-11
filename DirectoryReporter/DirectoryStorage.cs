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

        private int LastSentIndex;

        public DirectoryStorage()
        {
            OnPathRecived = new EventWaitHandle(false, EventResetMode.AutoReset);
            FileSystemEntities = new List<FileSystemInfo>();
        }

        public FileInfoFrame GetNextPath()
        {
            var result = new FileInfoFrame();
            if (FileSystemEntities.Count > LastSentIndex)
            {
                OnPathRecived.Set();
                result.FileSystemEntity = FileSystemEntities[LastSentIndex];
                Interlocked.Increment(ref LastSentIndex);
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
    }
}
