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
    class WatcherContest
    {
        private int Id { get; set; }
        private int WatcherId { get; set; }
        private Watcher Watcher { get; set; }
        private int ContestId { get; set; }
        private Contest Contest { get; set; }

        // metadata
        private DateTime Joined { get; set; }
    }
}
