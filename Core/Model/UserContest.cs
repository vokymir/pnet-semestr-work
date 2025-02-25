using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    /// <summary>
    /// Join Table for Users who have admin access to Contests.
    /// Multiple users can have admin access to one contest, but it always has one owner - stored in Contest.
    /// </summary>
    class UserContest
    {
        private int Id { get; set; }
        private int UserId { get; set; }
        private User User { get; set; } = null!;
        private int ContestId { get; set; }
        private Contest Contest { get; set; } = null!;

        // metadata
        private DateTime Added { get; set; } = DateTime.Now;
    }
}
