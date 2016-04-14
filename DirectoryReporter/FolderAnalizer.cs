using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace DirectoryReporter
{
    public class FolderAnalyzer
    {
        private static readonly FolderAnalyzer instance = new FolderAnalyzer();

        public DirectoryStorage Storage;

        public Thread work;

        private FolderAnalyzer() { }

        public delegate void PathPostingSatus(Object s, EventArgs e);
        public delegate void PathPostingMessage(Object s, TextEventArgs e);

        public event PathPostingSatus OnPathPostingStart;
        public event PathPostingSatus OnPathPostingFinish;
        public event PathPostingMessage OnPathPostingWarning;

        public string TargetDirectoryPath;

        public static FolderAnalyzer Instance
        {
            get
            {
                return instance;
            }
        }

        public void StartAnalyze()
        {
            Thread DirectoryScan = new Thread(() => DirectorySearch(TargetDirectoryPath)) { Name = "Folder scan thread" };
            DirectoryScan.Start();
            OnPathPostingStart(this, EventArgs.Empty);
        }
        /*
            Non recursive Breadth-first search is better.
        */
        private void DirectorySearch(string directoryPath)
        {
            //directoryPath = @"C:\Users\Iurii\Documents\My Videos";
            var stack = new Stack<string>();
            stack.Push(directoryPath);

            try
            {
                while (stack.Any())
                {
                    var current = stack.Pop();
                    try
                    {
                        if (current == TargetDirectoryPath)
                        {
                            Storage.PushDirectoryPath(current);
                        }
                        foreach (var f in Directory.GetFiles(current))
                        {
                            Storage.PushFilePath(f);
                        }
                        foreach (string d in Directory.GetDirectories(current))
                        {
                            stack.Push(d);
                            Storage.PushDirectoryPath(d);
                        }
                    }
                    catch (UnauthorizedAccessException ex_aue)
                    {
                        if (OnPathPostingWarning != null)
                        {
                            OnPathPostingWarning(this, new TextEventArgs(ex_aue.Message));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (OnPathPostingWarning != null)
                {
                    OnPathPostingWarning(this, new TextEventArgs(ex.Message));
                }
            }
            finally
            {
                Storage.PathReceivingCompleted();
                if (OnPathPostingFinish != null)
                {
                    OnPathPostingFinish(this, EventArgs.Empty);
                }
            }
        }
        /*
        private void DirectorySearchRecursivly(string directoryPath)
        {
            try
            {
                if (directoryPath == TargetDirectoryPath)
                {
                    Storage.PushDirectoryPath(directoryPath);
                }
                foreach (var f in Directory.GetFiles(directoryPath))
                {
                    Storage.PushFilePath(f);
                }
                foreach (string d in Directory.GetDirectories(directoryPath))
                {
                    Storage.PushDirectoryPath(d);
                    DirectorySearch(d);
                }
            }
            catch (UnauthorizedAccessException ex_aue)
            {
                if (OnPathPostingWarning != null)
                {
                    OnPathPostingWarning(this, new TextEventArgs(ex_aue.Message));
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            finally
            {
                if (directoryPath == TargetDirectoryPath)
                {
                    Storage.PathReceivingCompleted();
                    if (OnPathPostingFinish != null)
                    {
                        OnPathPostingFinish(this, EventArgs.Empty);
                    }
                }
            }
        }
        */
    }
}
