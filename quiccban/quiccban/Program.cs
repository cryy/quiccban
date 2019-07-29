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
using Newtonsoft.Json.Linq;
using quiccban.Services.Discord;

namespace quiccban
{
    public class Program
    {
        private static Logger _logger = new Logger("DILogger");
        private static Config _config;

        public static string dataPath = Path.GetFullPath("./data");

        public static void Main(string[] args)
            => new Program().MainAsync(args).GetAwaiter().GetResult();

        public async Task MainAsync(string[] args)
        {
            try
            {
                Console.WriteLine($"{FiggleFonts.Univers.Render("quiccban")}\n\n", Color.CadetBlue);

                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);

                if (!File.Exists(Path.Combine(dataPath, "config.json")))
                    await CreateConfig();

                dbcreation:
                if (!File.Exists(Path.Combine(dataPath, ".db")))
                {
                    File.Create(Path.Combine(dataPath, ".db")).Close();
                    File.SetAttributes(Path.Combine(dataPath, ".db"), FileAttributes.Hidden);
                }
                else if (args.Contains("restartdb"))
                {
                    File.Delete(Path.Combine(dataPath, ".db"));
                    goto dbcreation;
                }


                var configJObject = JObject.Parse(File.ReadAllText(Path.Combine(dataPath, "config.json")));

                var configResult = configJObject.ParseConfig();
                if (!configResult.IsValid)
                {
                    _logger.LogError($"Failed to parse config: {configResult.Message}");
                    Console.ReadKey();
                    Environment.Exit(13);
                }

                _config = configResult.ParsedConfig;


                var webhost = CreateWebHostBuilder(args).Build();

                if (_config.Web.Enabled)
                {
                    var addresser = webhost.ServerFeatures.FirstOrDefault(x => x.Value is IServerAddressesFeature).Value as IServerAddressesFeature;

                    _logger.LogInformation($"Starting webhost on: {string.Join(", ", addresser.Addresses)}");
                    await webhost.RunAsync();
                }
                else
                {
                    _logger.LogInformation("Not using Web UI as per config.");

                    webhost.Services.GetService<DiscordService>();
                    await Task.Delay(-1);

                }
            }
            catch (Exception e)
            {
                _logger.LogCritical("Failed to start webhost.; \n{0}", e.ToString());
                Console.ReadKey();
            }
        }

        private async Task CreateConfig()
        {
            await File.WriteAllTextAsync(dataPath + "/config.json", JsonBuilder.DefaultJsonConfig().ToString());
            _logger.LogCritical("Missing config file. One has been generated, please fill it out.");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .SuppressStatusMessages(true)
                .ConfigureServices(x => { x.AddSingleton(typeof(Config), _config); x.AddSingleton(typeof(ILogger), _logger); })
                .ConfigureLogging(x => {
                    x.ClearProviders();
                    x.AddProvider(new LoggingProvider());

                    if (!x.Services.BuildServiceProvider().GetService<IHostingEnvironment>().IsDevelopment())
                        x.AddFilter("Microsoft", LogLevel.Warning);

                })
                .UseUrls(_config.Web.Ports == null ? new string[] { } : _config.Web.Ports.Select(x => $"http://*:{x}").ToArray())
                .UseStartup<Startup>();
    }
}
