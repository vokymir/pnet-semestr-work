using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    class Watcher
    {
        private int Id { get; set; }
        private int ParentId { get; set; }
        private User Parent { get; set; }
        private string Name { get; set; }
    }
}
