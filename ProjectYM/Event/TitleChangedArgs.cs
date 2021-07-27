using System;

namespace ProjectYM.Event
{
    public class TitleChangedArgs : EventArgs
    {
        public string Title { get; set; }
        public TitleChangedArgs(string title)
        {
            Title = title;
        }
    }
}
