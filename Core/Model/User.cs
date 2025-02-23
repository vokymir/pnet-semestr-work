using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    /// <summary>
    /// User represents one physical person who can log in to the system.
    /// Each user can create multiple watchers and contests.
    /// </summary>
    class User
    {
        private int Id { get; set; }
        /// <summary>
        /// Login email for the user.
        /// </summary>
        private string Email { get; set; }
        /// <summary>
        /// Users hashed password..
        /// </summary>
        private string PasswordHash { get; set; }

        // All User metadata follows.

        private DateTime Created { get; set; }
    }
}
