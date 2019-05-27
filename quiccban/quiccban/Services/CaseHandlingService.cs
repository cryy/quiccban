using Discord;
using quiccban.Database;
using quiccban.Database.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActionType = quiccban.Database.Models.ActionType;
using quiccban.Services.Discord;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;

namespace quiccban.Services
{
    public class CaseHandlingService
    {
        private IServiceProvider _serviceProvider;
        private ConcurrentDictionary<Case, CancellationTokenSource> InmemoryExpiringCases;


        public CaseHandlingService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InmemoryExpiringCases = new ConcurrentDictionary<Case, CancellationTokenSource>();
        }



        public bool TryAdd(Case guildCase)
        {
            if (guildCase.ActionExpiry > 0 || guildCase.ActionType == ActionType.Warn)
            {

                var cts = new CancellationTokenSource();

                if (!InmemoryExpiringCases.TryAdd(guildCase, cts))
                {
                    cts.Dispose();
                    return false;
                }

                var ms = (int)(guildCase.ActionExpiry > 0 ? (guildCase.GetEndingTime() - DateTimeOffset.UtcNow).TotalMilliseconds : (guildCase.GetWarnEndingTime() - DateTimeOffset.UtcNow).TotalMilliseconds);
                _ = Task.Delay(ms, cts.Token).ContinueWith(_ => Resolve(guildCase.GuildId, guildCase.Id, true, false), TaskContinuationOptions.NotOnCanceled);

                return true;
            }
            else return false;
        }

        public async Task ResolveAsync(Case guildCase, IUser resolver, string reason, bool force)
        {
            if (resolver != null)
            {
                if (!InmemoryExpiringCases.TryRemove(guildCase, out CancellationTokenSource cts))
                    return;

                cts.Cancel();
                cts.Dispose();

                using (var guildStorage = new GuildStorage())
                {

                    var discordService = _serviceProvider.GetService<DiscordService>();

                    var guild = discordService.discordClient.GetGuild(guildCase.GuildId);
                    if (guild == null)
                        throw new NullReferenceException("Unknown guild.");

                    var dbService = _serviceProvider.GetService<DatabaseService>();
                    var helperService = _serviceProvider.GetService<HelperService>();
                    var config = _serviceProvider.GetService<Config>();


                    guildCase.Resolved = true;
                    guildCase.ForceResolved = true;

                    await dbService.UpdateCaseAsync(guildCase);

                    switch (guildCase.ActionType)
                    {
                        case ActionType.Warn:
                            await UpdateDiscordMessage(guildCase);
                            await dbService.CreateNewCaseAsync(guild, (reason == null ? $"``Responsible moderator please do {config.Prefix} reason {guildCase.Id} <reason>`` " : reason) + (guildCase.GetDiscordMessageLink() == null ? "" : (guildCase.Guild.LogStyle == LogStyle.Basic ? $"(Tied to Case #{guildCase.Id})" : $"(Tied to [Case #{guildCase.Id}]({guildCase.GetDiscordMessageLink()}))")), ActionType.Unwarn, 0, resolver.Id, guildCase.TargetId);
                            break;
                        case ActionType.Tempmute:
                        case ActionType.Mute:
                            await UpdateDiscordMessage(guildCase);
                            IRole muteRole = guild.GetRole(guildCase.Guild.MuteRoleId);
                            if (muteRole != null)
                            {
                                var target = guild.GetUser(guildCase.TargetId);
                                if (target != null)
                                    await target.RemoveRoleAsync(muteRole);
                            }

                            await dbService.CreateNewCaseAsync(guild, (reason == null ? $"``Responsible moderator please do {config.Prefix} reason {guildCase.Id} <reason>`` " : reason) + (guildCase.ActionType == ActionType.Tempmute ? (guildCase.GetDiscordMessageLink() == null ? "" : (guildCase.Guild.LogStyle == LogStyle.Basic ? $"(Tied to Case #{guildCase.Id})" : $"(Tied to [Case #{guildCase.Id}]({guildCase.GetDiscordMessageLink()}))")) : ""), ActionType.Unmute, 0, resolver.Id, guildCase.TargetId);
                            break;
                        case ActionType.Tempban:
                        case ActionType.Ban:
                            var bans = await guild.GetBansAsync();
                            if (bans.Any(x => x.User.Id == guildCase.TargetId))
                                await guild.RemoveBanAsync(guildCase.TargetId);
                            await UpdateDiscordMessage(guildCase);
                            await dbService.CreateNewCaseAsync(guild, (reason == null ? $"``Responsible moderator please do {config.Prefix} reason {guildCase.Id} <reason>`` " : reason) + (guildCase.ActionType == ActionType.Tempban ? (guildCase.GetDiscordMessageLink() == null ? "" : (guildCase.Guild.LogStyle == LogStyle.Basic ? $"(Tied to Case #{guildCase.Id})" : $"(Tied to [Case #{guildCase.Id}]({guildCase.GetDiscordMessageLink()}))")) : ""), ActionType.Unban, 0, resolver.Id, guildCase.TargetId);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
                await Resolve(guildCase.GuildId, guildCase.Id, false, force);
        }

        public IEnumerable<Case> GetCases()
            => InmemoryExpiringCases.Select(x => x.Key);

        public async Task UpdateDiscordMessage(Case guildCase)
        {
            if (guildCase.DiscordMessageId != 0)
            {
                var discordService = _serviceProvider.GetService<DiscordService>();
                var helperService = _serviceProvider.GetService<HelperService>();
                var dbguild = guildCase.Guild;

                var modlogChannel = discordService.discordClient.GetChannel(dbguild.LogChannelId) as SocketTextChannel;
                var msg = await modlogChannel.GetMessageAsync(guildCase.DiscordMessageId) as IUserMessage;
                switch (guildCase.Guild.LogStyle)
                {
                    case LogStyle.Basic:
                        await msg.ModifyAsync(async x => x.Content = await helperService.ConstructCaseMessageAsync(guildCase));
                        break;
                    case LogStyle.Modern:
                        var eb = new EmbedBuilder();
                        eb.WithTitle($"Case **{guildCase.Id}**  »  {guildCase.ActionType}");
                        eb.WithDescription(await helperService.ConstructCaseMessageAsync(guildCase));
                        await msg.ModifyAsync(x => { x.Content = null; x.Embed = eb.Build(); });
                        break;
                    default:
                        break;
                }
            }
        }


        private async Task Resolve(ulong guildId, int caseId, bool isInmemory, bool force)
        {


            using (var guildStorage = new GuildStorage())
            {
                if (isInmemory)
                {
                    var c = InmemoryExpiringCases.FirstOrDefault(x => x.Key.Id == caseId && x.Key.GuildId == guildId);
                    InmemoryExpiringCases.Remove(c.Key, out CancellationTokenSource cts);
                    cts.Dispose();
                }

                var discordService = _serviceProvider.GetService<DiscordService>();

                var guild = discordService.discordClient.GetGuild(guildId);
                if (guild == null)
                    throw new NullReferenceException("Unknown guild.");

                var dbService = _serviceProvider.GetService<DatabaseService>();
                var helperService = _serviceProvider.GetService<HelperService>();


                var dbguild = await guildStorage.GetOrCreateGuildAsync(guildId);

                var guildCase = dbguild.Cases.FirstOrDefault(x => x.Id == caseId);

                guildCase.Resolved = true;
                if (force)
                    guildCase.ForceResolved = true;

                await dbService.UpdateCaseAsync(guildCase);

                switch (guildCase.ActionType)
                {
                    case ActionType.Warn:
                        await UpdateDiscordMessage(guildCase);
                        await dbService.CreateNewCaseAsync(guild, "Warn expired. " + (guildCase.GetDiscordMessageLink() == null ? "" : (guildCase.Guild.LogStyle == LogStyle.Basic ? $" (Tied to Case #{guildCase.Id})" : $"(Tied to [Case #{guildCase.Id}]({guildCase.GetDiscordMessageLink()}))")), ActionType.Unwarn, 0, discordService.discordClient.CurrentUser.Id, guildCase.TargetId);
                        break;
                    case ActionType.Tempmute:
                        await UpdateDiscordMessage(guildCase);
                        IRole muteRole = guild.GetRole(dbguild.MuteRoleId);
                        if (muteRole != null)
                        {
                            var target = guild.GetUser(guildCase.TargetId);
                            if (target != null)
                                await target.RemoveRoleAsync(muteRole);
                        }

                        await dbService.CreateNewCaseAsync(guild, "Temporary mute expired. " + (guildCase.GetDiscordMessageLink() == null ? "" : (guildCase.Guild.LogStyle == LogStyle.Basic ? $" (Tied to Case #{guildCase.Id})" : $"(Tied to [Case #{guildCase.Id}]({guildCase.GetDiscordMessageLink()}))")), ActionType.Unmute, 0, discordService.discordClient.CurrentUser.Id, guildCase.TargetId);
                        break;
                    case ActionType.Tempban:
                        await UpdateDiscordMessage(guildCase);
                        await dbService.CreateNewCaseAsync(guild, "Temporary ban expired. " + (guildCase.GetDiscordMessageLink() == null ? "" : (guildCase.Guild.LogStyle == LogStyle.Basic ? $" (Tied to Case #{guildCase.Id})" : $"(Tied to [Case #{guildCase.Id}]({guildCase.GetDiscordMessageLink()}))")), ActionType.Unban, 0, discordService.discordClient.CurrentUser.Id, guildCase.TargetId);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
