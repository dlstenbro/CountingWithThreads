using System.Collections.Concurrent;

namespace CountingWithThreads.Models
{
    public class Entry
    {
        public string word { get; set; }
        public int count { get; set; }
    }
    public class WordList
    {
        private ConcurrentDictionary<string, int> entries;
        public ConcurrentDictionary<string, int> Entries
        {
            get 
            {
                if(entries == null)
                {
                    this.entries = new ConcurrentDictionary<string, int>();
                }
                return entries;
            }
            set
            {
                this.entries = value;
            }

        }
    }
}
