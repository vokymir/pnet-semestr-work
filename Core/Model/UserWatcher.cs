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
    public class UserWatcher
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int WatcherId { get; set; }
        public Watcher Watcher { get; set; } = null!;

        // metadata
        public DateTime Added { get; set; } = DateTime.Now;
    }
}
