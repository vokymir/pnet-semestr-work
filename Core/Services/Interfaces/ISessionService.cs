using Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Interfaces
{
    /// <summary>
    /// Service for managing the current session.
    /// </summary>
    public interface ISessionService
    {
        // To store only minimal required information
        int? CurrentUserId { get; set; }
    }
}
