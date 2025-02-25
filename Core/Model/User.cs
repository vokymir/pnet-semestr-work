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
    public class User
    {
        public int Id { get; init; }
        /// <summary>
        /// Login email for the user.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Users hashed password..
        /// </summary>
        public string PasswordHash { get; set; }


        public User(string Email, string PasswordHash)
        {
            this.PasswordHash = PasswordHash;
            this.Email = Email;
        }

        public override string ToString()
        {
            return $"{Id}: {Email} | {PasswordHash}";
        }


        // All User metadata follows.

        private DateTime Created { get; set; } = DateTime.Now;
    }
}
