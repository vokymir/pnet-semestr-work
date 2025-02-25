using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    /// <summary>
    /// Represents one entry of a bird in a contest.
    /// Primary key is a combination of ContestId, WatcherId and Timestamp. !!! NOT IMPLEMENTED YET !!!
    /// </summary>
    public class Entry
    {
        // primary key
        public int ContestId { get; set; }
        public Contest Contest { get; set; } = null!;
        public int WatcherId { get; set; }
        public Watcher Watcher { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Additional info
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;

        // bird may be a class, but for now it is a string
        public string Bird { get; set; } = string.Empty;
        // bird metadata
        public string Location { get; set; } = string.Empty;
        // if will implement
        public int Count { get; set; }

        // admin editable area - to avoid merge conflicts, users and admins can't edit the same fields
        public string AdminComment { get; set; } = string.Empty;
        public bool AdminDisapproved { get; set; }
    }
}
