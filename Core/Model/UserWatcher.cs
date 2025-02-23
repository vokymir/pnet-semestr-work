using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    class UserWatcher
    {
        private int Id { get; set; }
        private int UserId { get; set; }
        private User User { get; set; }
        private int WatcherId { get; set; }
        private Watcher Watcher { get; set; }
    }
}
