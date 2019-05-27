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
using Discord.Rest;

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

                    var unexpiredCases = guild.Cases.Where(x => (x.GetEndingTime() > DateTimeOffset.UtcNow && x.ActionType != ActionType.Warn) || (x.ActionType == ActionType.Warn && x.GetWarnEndingTime() > DateTimeOffset.UtcNow));

                    int failedCaseAdds = 0;
                    foreach(var guildCase in unexpiredCases)
                    {
                        Console.WriteLine($"caching {guildCase.Id}");
                        if (!_caseHandlingService.TryAdd(guildCase))
                            failedCaseAdds++;
                    }

                    if (failedCaseAdds > 0)
                        _logger.LogWarning($"Failed to add {failedCaseAdds} cases to in-memory cache.");

                    //resolve all cases that had expired
                    var unresolvedExpiredCases = guild.Cases.Where(x => !x.Resolved && ((x.ActionType != ActionType.Warn && x.GetEndingTime() <= DateTimeOffset.UtcNow )|| (x.GetWarnEndingTime() <= DateTimeOffset.UtcNow && x.ActionType == ActionType.Warn)));

                    foreach (var guildCase in unresolvedExpiredCases)
                    {
                        Console.WriteLine($"resolving {guildCase.Id}");
                        await _caseHandlingService.ResolveAsync(guildCase, null, null, false);
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

                    if (result is FailedResult tpf)
                    {
                        await context.Channel.SendMessageAsync(tpf.Reason);
                    }
                }



            };

            discordClient.UserBanned += async (u, g) =>
            {
                await Task.Delay(500);

                var auditLogs = await g.GetAuditLogsAsync(20).FlattenAsync();
                var auditLog = auditLogs.FirstOrDefault(a => a.Data is BanAuditLogData ban && ban.Target.Id == u.Id);
                if (auditLog == null)
                    return;

                if (auditLog.User.Id == discordClient.CurrentUser.Id)
                    return;

                var data = auditLog.Data as BanAuditLogData;
                var dbService = _serviceProvider.GetService<DatabaseService>();

                await dbService.CreateNewCaseAsync(g, auditLog.Reason, ActionType.Ban, 0, auditLog.User.Id, data.Target.Id);

            };

            discordClient.UserUnbanned += async (u, g) =>
            {
                await Task.Delay(500);

                var auditLogs = await g.GetAuditLogsAsync(20).FlattenAsync();
                var auditLog = auditLogs.FirstOrDefault(a => a.Data is UnbanAuditLogData unban && unban.Target.Id == u.Id);
                if (auditLog == null)
                    return;

                if (auditLog.User.Id == discordClient.CurrentUser.Id)
                    return;

                var data = auditLog.Data as UnbanAuditLogData;
                var dbService = _serviceProvider.GetService<DatabaseService>();
                var caseService = _serviceProvider.GetService<CaseHandlingService>();

                var tempcase = caseService.GetCases().FirstOrDefault(x => x.GuildId == g.Id && x.TargetId == data.Target.Id && x.ActionType == ActionType.Tempban && !x.Resolved);
                if (tempcase != null)
                    await _caseHandlingService.ResolveAsync(tempcase, auditLog.User, auditLog.Reason, true);
                else
                    await dbService.CreateNewCaseAsync(g, auditLog.Reason, ActionType.Ban, 0, auditLog.User.Id, data.Target.Id);
            };

            discordClient.UserLeft += async (u) =>
            {
                await Task.Delay(500);

                var auditLogs = await u.Guild.GetAuditLogsAsync(20).FlattenAsync();
                var auditLog = auditLogs.FirstOrDefault(a => a.Data is KickAuditLogData kick && kick.Target.Id == u.Id);
                if (auditLog == null)
                    return;

                if (auditLog.User.Id == discordClient.CurrentUser.Id)
                    return;

                var data = auditLog.Data as KickAuditLogData;
                var dbService = _serviceProvider.GetService<DatabaseService>();

                await dbService.CreateNewCaseAsync(u.Guild, auditLog.Reason, ActionType.Kick, 0, auditLog.User.Id, data.Target.Id);
            };

            discordClient.GuildMemberUpdated += async (u_before, u_after) =>
            {
                await Task.Delay(500);

                var auditLogs = await u_after.Guild.GetAuditLogsAsync(20).FlattenAsync();
                var auditLog = auditLogs.FirstOrDefault(a => a.Data is MemberRoleAuditLogData role && role.Target.Id == u_after.Id);
                if (auditLog == null)
                    return;

                if (auditLog.User.Id == discordClient.CurrentUser.Id)
                    return;

                var data = auditLog.Data as MemberRoleAuditLogData;

                var dbService = _serviceProvider.GetService<DatabaseService>();
                var caseService = _serviceProvider.GetService<CaseHandlingService>();
                var dbGuild = await dbService.GetOrCreateGuildAsync(u_after.Guild);

                if(data.Roles.Any(x => x.RoleId == dbGuild.MuteRoleId && x.Added))
                {
                    await dbService.CreateNewCaseAsync(u_after.Guild, auditLog.Reason, ActionType.Mute, 0, auditLog.User.Id, data.Target.Id);
                }
                else if(data.Roles.Any(x => x.RoleId == dbGuild.MuteRoleId && !x.Added))
                {
                    var tempcase = caseService.GetCases().FirstOrDefault(x => x.TargetId == data.Target.Id && x.ActionType == ActionType.Tempmute && !x.Resolved);
                    if (tempcase != null)
                        await _caseHandlingService.ResolveAsync(tempcase, auditLog.User, auditLog.Reason, true);
                    else
                        await dbService.CreateNewCaseAsync(u_after.Guild, auditLog.Reason, ActionType.Unmute, 0, auditLog.User.Id, data.Target.Id);
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
            if (_config.AllowMentionPrefix)
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
            _commands.AddTypeParser(new CaseParser());
            _commands.AddTypeParser(new TimeSpanParser());
            _commands.AddTypeParser(new BanParser());
            

            _commands.AddModules(Assembly.GetEntryAssembly(), action: m =>
            {

            });
        }
    }
}
