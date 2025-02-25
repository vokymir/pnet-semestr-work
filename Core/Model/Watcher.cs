using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    /// <summary>
    /// Represents one physical person, one bird watcher.
    /// Each user can have multiple watchers, as well as one watcher can be controled by multiple users.
    /// Watcher can join multiple contests.
    /// </summary>
    public class Watcher
    {
        public int Id { get; set; }
        /// <summary>
        /// Initially user who created this watcher.
        /// Parenthood can be transfered, but ultimately there is always one parent.
        /// </summary>
        public int ParentId { get; set; }
        public User Parent { get; set; } = null!;

        // All Watcher metadata follows.
        
        public string Name { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
