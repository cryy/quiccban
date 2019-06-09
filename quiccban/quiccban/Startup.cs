using Discord.Addons.Interactive;
using Humanizer.Configuration;
using Humanizer.DateTimeHumanizeStrategy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Qmmands;
using quiccban.API;
using quiccban.Database;
using quiccban.Logging;
using quiccban.Services;
using quiccban.Services.Discord;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace quiccban
{
    public class Startup
    {
        public Startup(ILogger logger)
        {
            Logger = logger;
        }

        public ILogger Logger;


        public void ConfigureServices(IServiceCollection services)
        {
            var configJObject = JObject.Parse(File.ReadAllText(Program.dataPath + "/config.json"));

            var configResult = configJObject.ParseConfig();
            if (!configResult.IsValid)
            {
                Logger.LogError($"Failed to parse config: {configResult.Message}");
                Console.ReadKey();
                Environment.Exit(13);
            }

            Configurator.DateTimeOffsetHumanizeStrategy = new PrecisionDateTimeOffsetHumanizeStrategy(.5);

            var commandService = new CommandService(new CommandServiceConfiguration {
                IgnoreExtraArguments = true
            });
            var responseService = new ResponseService(Logger);

            if (File.Exists(Program.dataPath + "/responses.json"))
                responseService.Load();
            
            services.AddSingleton(configResult.ParsedConfig);
            services.AddSingleton(commandService);
            services.AddSingleton(responseService);
            services.AddSingleton((provider) => new DiscordService(provider));
            services.AddSingleton((provider) => new CaseHandlingService(provider));
            services.AddSingleton((provider) => new DatabaseService(provider));
            services.AddSingleton((provider) => new HelperService(provider));
            services.AddSingleton((provider) =>
            {
                var discordService = provider.GetService<DiscordService>();

                return new InteractiveService(discordService.discordClient);
            });
            services.AddDbContext<GuildStorage>(ServiceLifetime.Transient);


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = Path.Combine(Assembly.GetEntryAssembly().Location, "../ClientApp/build");
            });




        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            try
            {
                using (var guildStorage = new GuildStorage())
                {
                    Logger.LogInformation("Ensuring database is created.");
                    guildStorage.Database.EnsureCreated();
                    Logger.LogInformation("Success.");

                }
            }
            catch
            {
                Logger.LogCritical("Failed to generate/load database. Exiting.");
                Environment.Exit(0);
            }

            app.ApplicationServices.GetService<DiscordService>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = Path.Combine(Assembly.GetEntryAssembly().Location, "ClientApp");

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
