using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    class User
    {
        private int Id { get; set; }
        private string Email { get; set; }
        private string PasswordHash { get; set; }
    }
}
