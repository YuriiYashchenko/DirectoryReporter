using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DirectoryReporter
{
    public class DirectoryStorage
    {
        public EventWaitHandle OnPathRecived;

        private List<string> Pathes;

        private volatile bool IsPathReceivingCompleted;

        public void PathReceivingCompleted() {
            IsPathReceivingCompleted = true;
            OnPathRecived.Set();
        }

        private int LastSentIndex;

        public DirectoryStorage()
        {
            OnPathRecived = new EventWaitHandle(false, EventResetMode.AutoReset);
            Pathes = new List<string>();
        }

        public string GetNextPath()
        {
            string result = String.Empty;

            if (Pathes.Count > LastSentIndex)
            {
                OnPathRecived.Set();
                result = Pathes[LastSentIndex];               
                Interlocked.Increment(ref LastSentIndex);
            }
            else {
                if (IsPathReceivingCompleted)
                {
                    OnPathRecived.Set();
                    return null;
                }
            }            
            return result;
        }

        public void PushDirectoryPath(string path)
        {
            Monitor.Enter(this.Pathes);
            Pathes.Add(path);
            Monitor.Exit(this.Pathes);
            OnPathRecived.Set();
        }
    }
}
