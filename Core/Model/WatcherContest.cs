using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    /// <summary>
    /// Join table for Watchers in Contests.
    /// One Watcher can join multiple contests, and one contest can have multiple watchers.
    /// </summary>
    public class WatcherContest
    {
        public int Id { get; set; }
        public int WatcherId { get; set; }
        public Watcher Watcher { get; set; } = null!;
        public int ContestId { get; set; }
        public Contest Contest { get; set; } = null!;

        // metadata
        public DateTime Joined { get; set; } = DateTime.Now;
    }
}
