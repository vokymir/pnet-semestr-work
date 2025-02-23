using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    class UserContest
    {
        private int Id { get; set; }
        private int UserId { get; set; }
        private User User { get; set; }
        private int ContestId { get; set; }
        private Contest Contest { get; set; }
    }
}
