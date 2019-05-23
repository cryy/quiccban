using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using quiccban.API;
using System.Reflection;
using quiccban.Services.Discord.Commands;
using quiccban.Database;
using ActionType = quiccban.Database.Models.ActionType;

namespace quiccban.Services.Discord
{
    public class DiscordService
    {
        private Config _config;
        private IHubContext<SocketHub> _hubContext;
        private ILogger _logger;
        private CommandService _commands;
        private IServiceProvider _serviceProvider;
        private List<string> _prefixes;
        private CaseHandlingService _caseHandlingService;


        public DiscordSocketClient discordClient;
        
        public bool IsReady { get; private set; }

        public DiscordService(IServiceProvider serviceProvider)
        {
            discordClient = new DiscordSocketClient();
            _config = serviceProvider.GetService<Config>();
            _hubContext = serviceProvider.GetService<IHubContext<SocketHub>>();
            _logger = serviceProvider.GetService<ILogger>();
            _commands = serviceProvider.GetService<CommandService>();
            _caseHandlingService = serviceProvider.GetService<CaseHandlingService>();
            _serviceProvider = serviceProvider;
            _prefixes = new List<string>();

            IsReady = false;
            

            Task.Run(async () =>
            {
                await StartAsync();
            });
        }


        private async Task StartAsync()
        {
            async Task LoginRoutine()
            {
                await discordClient.LoginAsync(TokenType.Bot, _config.DiscordToken);
                await discordClient.StartAsync();

                await Task.Delay(-1);
            }

            discordClient.Ready += async () =>
            {
                _logger.LogInformation($"Logged into Discord as \"{discordClient.CurrentUser}\" in {discordClient.Guilds.Count} guild{(discordClient.Guilds.Count > 1 ? "s" : "")}.");

                LoadCommands();

                var guildStorage = _serviceProvider.GetService<GuildStorage>();

                var dbGuilds = await guildStorage.GetAllGuildsAsync();

                foreach(var guild in dbGuilds)
                {

                    var unexpiredCases = guild.Cases.Where(x => x.GetEndingTime() > DateTimeOffset.UtcNow || x.ActionType == ActionType.Warn);

                    int failedCaseAdds = 0;
                    foreach(var guildCase in unexpiredCases)
                    {
                        if (!_caseHandlingService.TryAdd(guildCase))
                            failedCaseAdds++;
                    }

                    if (failedCaseAdds > 0)
                        _logger.LogWarning($"Failed to add {failedCaseAdds} cases to in-memory cache.");

                    //resolve all cases that had expired
                    var unresolvedExpiredCases = guild.Cases.Where(x => !x.Resolved && (x.GetEndingTime() <= DateTimeOffset.UtcNow));

                    foreach (var guildCase in unresolvedExpiredCases)
                    {
                        await _caseHandlingService.ResolveAsync(guildCase);
                    }
                   
                }

                IsReady = true;
            };

            discordClient.MessageReceived += async (m) =>
            {
                if (m.Channel is IDMChannel) return;
                if (!(m is SocketUserMessage msg)) return;

                if (CommandUtilities.HasAnyPrefix(msg.Content, _prefixes, StringComparison.OrdinalIgnoreCase, out string pfx, out string output))
                {
                    var context = new QuiccbanContext(discordClient, msg);
                    var result = await _commands.ExecuteAsync(output, context, _serviceProvider);
                }

            };




            _logger.LogInformation("Attempting to log into Discord...");

            try
            {
                await LoginRoutine();
            }
            catch
            {
                _logger.LogError("Failed to log into Discord. Attempting reconnect in 10 seconds.");
                await Task.Delay(10000);
                try
                {
                    await LoginRoutine();
                }
                catch
                {
                    _logger.LogError("Reconnection failed. Exiting.");
                    Environment.Exit(0);
                }
            }
        }

        private void LoadCommands()
        {
            _prefixes.Add(_config.Prefix);
            if(_config.AllowMentionPrefix)
            {
                _prefixes.Add($"<@{discordClient.CurrentUser.Id}>");
                _prefixes.Add($"<@!{discordClient.CurrentUser.Id}>");
            }

            _logger.LogInformation($"Using {_prefixes.Count} prefix{(_prefixes.Count > 1 ? "es" : "")}; {string.Join(", ", _prefixes)}.");

            _commands.AddTypeParser(new SocketUserParser<SocketUser>());
            _commands.AddTypeParser(new SocketUserParser<SocketGuildUser>());
            _commands.AddTypeParser(new SocketGuildChannelParser<SocketGuildChannel>());
            _commands.AddTypeParser(new SocketGuildChannelParser<SocketTextChannel>());
            _commands.AddTypeParser(new SocketGuildChannelParser<SocketCategoryChannel>());
            _commands.AddTypeParser(new SocketGuildChannelParser<SocketVoiceChannel>());
            _commands.AddTypeParser(new SocketRoleParser());

            _commands.AddModules(Assembly.GetEntryAssembly(), action: m => {
                
            });

        }
    }
}
