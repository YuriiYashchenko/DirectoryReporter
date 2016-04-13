using System;

namespace DirectoryReporter
{
    public class TextEventArgs : EventArgs
    {
        public TextEventArgs(string text)
        {
            this.Text = text;
        }
        public string Text
        {
            get;
            private set;
        }
    }
}
