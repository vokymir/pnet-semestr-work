using Core.Data;
using Core.Services;
using Core.Services.Interfaces;
using Core.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core
{
    internal class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// Sets up the Dependency Injection container and runs the application.
        /// </summary>
        /// <param name="args">Nothing yet...</param>
        static void Main(string[] args)
        {
            // This is basically a Dependency Injection Container
            var serviceCollection = new ServiceCollection();
            // There is a method that configures the services
            ConfigureServices(serviceCollection);

            // This provider will create instances of the services on demand, with the DI already done
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Creating an instance of the ConsoleUI, which will have all its dependencies injected
            var ui = serviceProvider.GetService<ConsoleUI>();
            // Running the Application
            if (ui == null)
            {
                throw new Exception("UI is null.");
            }
            ui.Run(args);
        }

        /// <summary>
        /// Configures the services that will have the Dependency injection setup.
        /// </summary>
        /// <param name="services"></param>
        private static void ConfigureServices(ServiceCollection services)
        {
            // add database
            services.AddDbContext<AppDbContext>();

            // add singleton = only one instance of the service will be created
            services.AddSingleton<ISessionService, SessionService>();

            // add transient = a new instance of the service will be created every time it is requested
            services.AddTransient<UserService>();
            services.AddTransient<EntryService>();
            services.AddTransient<ContestService>();
            services.AddTransient<WatcherService>();
            
            services.AddTransient<ConsoleUI>();
        }
    }
}
