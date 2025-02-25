using Core.Data;
using Core.Model;
using Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    /// <summary>
    /// Registering, logging, validating email, hashing passwords.
    /// </summary>
    public class UserService
    {
        private readonly AppDbContext _dbContext;
        private readonly ISessionService _sessionService;

        public UserService(AppDbContext appDbContext, ISessionService sessionService)
        {
            _dbContext = appDbContext;
            _sessionService = sessionService;
        }


        public async void AddUserAsync(string email, string passwordHash, int id)
        {
            var user = new User(email, passwordHash) { Id = id };
            Console.WriteLine(user);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }
        public List<User> GetUsers()
        {
            return _dbContext.Users.ToList();
        }
    }
}
