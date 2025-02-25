using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    public enum ContestVisibility
    {
        Private = 0,
        InviteOnly,
        Public,
    }
    /// <summary>
    /// Represents one real event.
    /// Multiple users can have admin access to one contest, but it always has one owner.
    /// Multiple watchers can join one contest.
    /// </summary>
    public class Contest
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        // metadata

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        // concept: if set to public, anybody can join
        public ContestVisibility Visibility { get; set; }
        // tracking time of creation
        public DateTime Created { get; set; } = DateTime.Now;
        // tracking time of contest start and end
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        // the deadline for watchers to upload birds - can differ from contest end
        public DateTime UploadDeadline { get; set; }

        public int MaxWatchers { get; set; }
    }
}
