using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DirectoryReporter
{
    public class DirectoryStorage
    {
        private List<string> Pathes;

        private int LastSentIndex;

        private int PathesCount;

        public delegate void PathPosting (Object s, PathPushedEventArgs e);

        public event PathPosting OnPathPushed;

        public DirectoryStorage()
        {
            Pathes = new List<string>();
        }
        
        public string GetNextPath() {
            string result = String.Empty;
            if (LastSentIndex < Pathes.Count)
            {
                result = Pathes[LastSentIndex];
                Interlocked.Increment(ref LastSentIndex);
            }
            return result;
        }

        public void PushDirectoryPath(string path){
            Monitor.Enter(this.Pathes);
            Pathes.Add(path);
            Monitor.Exit(this.Pathes);
            
            //OnPathPushed(this, new PathPushedEventArgs() { Message = path });
        }        
    }
}
