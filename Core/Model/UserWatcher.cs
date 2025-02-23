using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    /// <summary>
    /// Join table for Users and their Watchers.
    /// Each User can have multiple Watchers, and each Watcher can be controled by multiple Users.
    /// </summary>
    class UserWatcher
    {
        private int Id { get; set; }
        private int UserId { get; set; }
        private User User { get; set; }
        private int WatcherId { get; set; }
        private Watcher Watcher { get; set; }

        // metadata
        private DateTime Added { get; set; }
    }
}
