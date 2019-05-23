using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using quiccban.Logging;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Figgle;
using Console = Colorful.Console;
using System.Drawing;
using quiccban.Assets;

namespace quiccban
{
    public class Program
    {
        private static Logger _logger = new Logger("DILogger");

        public static string dataPath = Path.GetFullPath("./data");
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine($"{FiggleFonts.Univers.Render("quiccban")}\n\n", Color.CadetBlue);

                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);

                if(!File.Exists(dataPath + "/config.json"))
                    CreateConfig();

                var webhost = CreateWebHostBuilder(args).Build();
                var addresser = webhost.ServerFeatures.FirstOrDefault(x => x.Value is IServerAddressesFeature).Value as IServerAddressesFeature;

                _logger.LogInformation($"Starting webhost on: {string.Join(", ", addresser.Addresses)}");


                webhost.Run();
            }
            catch (Exception e)
            {
                _logger.LogCritical("Failed to start webhost.; \n{0}", e.ToString());
                Console.ReadKey();
            }
        }

        private static void CreateConfig()
        {
            File.WriteAllText(dataPath + "/config.json", JsonBuilder.DefaultJsonConfig().ToString());
            _logger.LogCritical("Missing config file. One has been generated, please fill it out.");
            Console.ReadKey();
            Environment.Exit(0);
        }


        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .SuppressStatusMessages(true)
                .UseConfiguration(new ConfigurationBuilder().AddJsonFile(dataPath + "/config.json").Build())
                .ConfigureLogging(x => {
                    x.ClearProviders();
                    x.AddProvider(new LoggingProvider());

                    x.Services.AddSingleton(typeof(ILogger), _logger);
                })
                .UseStartup<Startup>();
    }
}
