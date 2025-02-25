using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    enum ContestVisibility
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
    class Contest
    {
        private int Id { get; set; }
        private int OwnerId { get; set; }
        private User Owner { get; set; } = null!;

        // metadata

        private string Name { get; set; } = string.Empty;
        private string Description { get; set; } = string.Empty;
        // concept: if set to public, anybody can join
        private ContestVisibility Visibility { get; set; }
        // tracking time of creation
        private DateTime Created { get; set; } = DateTime.Now;
        // tracking time of contest start and end
        private DateTime Start { get; set; }
        private DateTime End { get; set; }
        // the deadline for watchers to upload birds - can differ from contest end
        private DateTime UploadDeadline { get; set; }

        private int MaxWatchers { get; set; }
    }
}
