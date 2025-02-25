using Core.Model;
using Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    /// <summary>
    /// Implementation of the session service. 
    /// This is here because I will probably use the browser session and this should scale better...
    /// </summary>
    class SessionService : ISessionService
    {
        public int? CurrentUserId { get; set; }
    }
}
