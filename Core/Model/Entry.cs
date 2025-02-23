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
    class Entry
    {
        // primary key
        private int ContestId { get; set; }
        private Contest Contest { get; set; }
        private int WatcherId { get; set; }
        private Watcher Watcher { get; set; }
        private DateTime Timestamp { get; set; }

        // Additional info
        private int CreatedById { get; set; }
        private User CreatedBy { get; set; }

        // bird may be a class, but for now it is a string
        private string Bird { get; set; }
        // bird metadata
        private string Location { get; set; }
        // if will implement
        private int Count { get; set; }

        // admin editable area - to avoid merge conflicts, users and admins can't edit the same fields
        private string AdminComment { get; set; }
        private bool AdminDisapproved { get; set; }
    }
}
