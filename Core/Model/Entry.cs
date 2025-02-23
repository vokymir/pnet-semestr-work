using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    class Entry
    {
        private int ContestId { get; set; }
        private Contest Contest { get; set; }
        private int WatcherId { get; set; }
        private Watcher Watcher { get; set; }
        private string Bird { get; set; }
        private string Location { get; set; }
        private DateTime Timestamp { get; set; }
        private string AdminComment { get; set; }
        private bool AdminDisapproved { get; set; }
    }
}
