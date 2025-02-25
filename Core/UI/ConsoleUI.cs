using Core.Data;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.UI
{
    public class ConsoleUI
    {
        private readonly UserService _userService;

        public ConsoleUI(UserService authService)
        {
            _userService = authService;
        }

        public void Run(string[] args)
        {
            Console.WriteLine("Hello World from ConsoleUI!");
            _userService.AddUserAsync("voky", "aha", 1);
            _userService.AddUserAsync("vok", "aa", 2);
            Console.WriteLine("START");
            DisplayUsers();
            Console.WriteLine("WHAT");
        }

        public void DisplayUsers()
        {
           foreach(var user in _userService.GetUsers())
            {
                Console.WriteLine(user);
            } 
        }
    }
}
