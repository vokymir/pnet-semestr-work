using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    enum ContestVisibility
    {
        Public = 0,
        Private
    }
    class Contest
    {
        private int Id { get; set; }
        private ContestVisibility Visibility { get; set; }
    }
}
