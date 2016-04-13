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
        public event PathPostingSatus OnPathPostingStart;
        public event PathPostingSatus OnPathPostingFinish;

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
            directoryPath = @"C:\Users\iiashchenko\Desktop\Google-Dotnet-Samples-master\Google-Dotnet-Samples-master\Google-Site-Verification\SiteVerification-Sample\Daimto-SiteVerification\packages\Microsoft.Bcl.Async.1.0.168\lib\portable-net45+win8+wp8+wpa81";
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
                            if (f.Length < 261)
                            {
                                Storage.PushFilePath(f);
                                // Rise message or log problem
                            }
                            else
                            {
                                int tti = 0;
                            }                           
                        }
                        foreach (string d in Directory.GetDirectories(current))
                        {                            
                            if (d.Length < 261)
                            {
                                stack.Push(d);
                                Storage.PushDirectoryPath(d);
                                // Rise message or log problem
                            }
                            else
                            {
                                int tti = 0;
                            }
                        }
                    }
                    catch (PathTooLongException ex_longpath)
                    {
                        // Rise message or log problem
                        string s = ex_longpath.Message;
                    }
                    catch (UnauthorizedAccessException ex_aue)
                    {
                        // Rise message or log problem
                        string s = ex_aue.Message;
                    }
                    int ti = 0;
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
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
                int tti = 1;
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

    }
}
