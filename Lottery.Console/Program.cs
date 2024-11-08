using Lottery.Console;
using Lottery.Console.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Optionally, add more configuration sources here
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Register configuration settings
                    services.Configure<GameSettings>(context.Configuration.GetSection("GameSettings"));

                    // Register services
                    services.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();
                    services.AddSingleton<IUserInterface, ConsoleUserInterface>();

                    services.AddTransient<LotteryGame>();

                    // Set up the app's startup logic
                    services.AddHostedService<LotteryGameService>();
                }).Build();

host.Start();