using Discord.Addons.Interactive;
using Humanizer.Configuration;
using Humanizer.DateTimeHumanizeStrategy;
using Microsoft.AspNetCore.Authentication.Cookies;
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
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using quiccban.API.Filters;

namespace quiccban
{
    public class Startup
    {
        public Startup(ILogger logger, Config config)
        {
            Logger = logger;
            Config = config;
        }

        public ILogger Logger;
        public Config Config;


        public void ConfigureServices(IServiceCollection services)
        {

            Configurator.DateTimeOffsetHumanizeStrategy = new PrecisionDateTimeOffsetHumanizeStrategy(.5);

            var commandService = new CommandService(new CommandServiceConfiguration
            {
                IgnoreExtraArguments = true,
            });
            var responseService = new ResponseService(Logger);
            var oauthCachingService = new OAuthCachingService();


            if (File.Exists(Program.dataPath + "/responses.json"))
                responseService.Load();

            services.AddResponseCompression();
            services.AddSignalR();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = new TimeSpan(7, 0, 0, 0);
                    
                }).AddOAuth<DiscordAuthenticationOptions, DiscordAuthenticationHandler>(DiscordAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async (ctx) =>
                        {
                            await oauthCachingService.GetOrCreateClient(ctx.AccessToken);
                            ctx.Identity.AddClaim(new Claim("accessToken", ctx.AccessToken));
                        },
                        
                        
                    };
                    
                    options.ClientId = Config.Web.ClientId.ToString();
                    options.ClientSecret = Config.Web.ClientSecret;
                    options.CallbackPath = "/discord-auth";
                    

                    options.Scope.Add("identify");
                    options.Scope.Add("guilds");
                    


                });
            services.AddSingleton(commandService);
            services.AddSingleton(responseService);
            services.AddSingleton(oauthCachingService);
            services.AddSingleton((provider) => new DiscordService(provider));
            services.AddSingleton((provider) => new CaseHandlingService(provider));
            services.AddSingleton((provider) => new DatabaseService(provider));
            services.AddSingleton((provider) => new HelperService(provider));
            services.AddSingleton((provider) =>
            {
                var discordService = provider.GetService<DiscordService>();

                return new InteractiveService(discordService.discordClient);
            });
            services.AddScoped<RequireAuthAttribute>();
            services.AddScoped<RequireReadyClientAttribute>();
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
            catch
            {
                Logger.LogCritical("Failed to generate/load database. Exiting.");
                Environment.Exit(0);
            }

            app.ApplicationServices.GetService<DiscordService>();

            var options = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };

            app.UseForwardedHeaders(options);

            app.UseAuthentication();
            app.UseResponseCompression();

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


            app.UseSignalR(routes =>
            {
                routes.MapHub<SocketHub>("/api/ws");
            });

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
