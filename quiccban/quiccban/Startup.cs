using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qmmands;
using quiccban.API;
using quiccban.Database;
using quiccban.Logging;
using quiccban.Services;
using quiccban.Services.Discord;
using System;
using System.Threading.Tasks;

namespace quiccban
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public ILogger Logger;
        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            var configResult = Configuration.ParseConfig();
            if (!configResult.IsValid)
            {
                Logger.LogError($"Failed to parse config: {configResult.Message}");
                Environment.Exit(13);
            }

            var commandService = new CommandService(new CommandServiceConfiguration {
                IgnoreExtraArguments = true
            });
            
            services.AddSingleton(typeof(Config), configResult.ParsedConfig);
            services.AddSingleton(typeof(CommandService), commandService);
            services.AddSingleton((provider) => new DiscordService(provider));
            services.AddSingleton((provider) => new CaseHandlingService(provider));
            services.AddSingleton((provider) => new DatabaseService(provider));
            services.AddDbContext<GuildStorage>(ServiceLifetime.Transient);


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
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
            catch(Exception e)
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
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
