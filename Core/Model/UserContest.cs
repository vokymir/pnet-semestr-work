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
    public class UserContest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int ContestId { get; set; }
        public Contest Contest { get; set; } = null!;

        // metadata
        public DateTime Added { get; set; } = DateTime.Now;
    }
}
