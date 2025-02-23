using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    class WatcherContest
    {
        private int Id { get; set; }
        private int WatcherId { get; set; }
        private Watcher Watcher { get; set; }
        private int ContestId { get; set; }
        private Contest Contest { get; set; }
        private DateTime Joined { get; set; }
    }
}
