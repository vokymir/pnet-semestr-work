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
    class Watcher
    {
        private int Id { get; set; }
        /// <summary>
        /// Initially user who created this watcher.
        /// Parenthood can be transfered, but ultimately there is always one parent.
        /// </summary>
        private int ParentId { get; set; }
        private User Parent { get; set; }

        // All Watcher metadata follows.
        
        private string Name { get; set; }
        private DateTime Created { get; set; }
    }
}
