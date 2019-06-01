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
using Humanizer;

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

        public async Task ResolveAsync(Case guildCase, IUser resolver, string reason, bool force, bool doLock)
        {
            if (resolver != null)
            {
                var inmem = InmemoryExpiringCases.FirstOrDefault(x => x.Key.Id == guildCase.Id && x.Key.GuildId == guildCase.GuildId);
                if (inmem.Key == null)
                    return;

                if (!InmemoryExpiringCases.TryRemove(inmem.Key, out CancellationTokenSource cts))
                    return;

                cts.Cancel();
                cts.Dispose();


                using (var guildStorage = new GuildStorage())
                {
                    var discordService = _serviceProvider.GetService<DiscordService>();

                    var guild = discordService.discordClient.GetGuild(guildCase.GuildId);
                    if (guild == null)
                        throw new NullReferenceException("Unknown guild.");

                    var currentUser = guild.CurrentUser;

                    if (!currentUser.GuildPermissions.BanMembers || !currentUser.GuildPermissions.ManageRoles || !currentUser.GuildPermissions.KickMembers || !currentUser.GuildPermissions.ManageChannels)
                        throw new InvalidOperationException("Don't have enough permissions.");



                    var dbService = _serviceProvider.GetService<DatabaseService>();
                    var helperService = _serviceProvider.GetService<HelperService>();
                    var config = _serviceProvider.GetService<Config>();


                    guildCase.Resolved = true;
                    guildCase.ForceResolved = true;

                    await dbService.UpdateCaseAsync(guildCase, doLock);


                    switch (guildCase.ActionType)
                    {
                        case ActionType.Warn:
                            await UpdateDiscordMessage(guildCase);
                            await dbService.CreateNewCaseAsync(guild, (reason == null ? $"``Responsible moderator please do {config.Prefix} reason {{0}} <reason>``" : reason) + (guildCase.GetDiscordMessageLink() == null ? "" : (guildCase.Guild.LogStyle == LogStyle.Basic ? $" (Tied to Case #{guildCase.Id})" : $"(Tied to [Case #{guildCase.Id}]({guildCase.GetDiscordMessageLink()}))")), ActionType.Unwarn, 0, resolver.Id, guildCase.TargetId, doLock);
                            break;
                        case ActionType.TempMute:
                        case ActionType.Mute:
                            await UpdateDiscordMessage(guildCase);
                            IRole muteRole = guild.GetRole(guildCase.Guild.MuteRoleId);
                            if (muteRole != null)
                            {
                                var target = guild.GetUser(guildCase.TargetId);
                                if (target != null)
                                    await target.RemoveRoleAsync(muteRole);
                            }

                            await dbService.CreateNewCaseAsync(guild, (reason == null ? $"``Responsible moderator please do {config.Prefix} reason {{0}} <reason>``" : reason) + (guildCase.ActionType == ActionType.TempMute ? (guildCase.GetDiscordMessageLink() == null ? "" : (guildCase.Guild.LogStyle == LogStyle.Basic ? $" (Tied to Case #{guildCase.Id})" : $"(Tied to [Case #{guildCase.Id}]({guildCase.GetDiscordMessageLink()}))")) : ""), ActionType.Unmute, 0, resolver.Id, guildCase.TargetId, doLock);
                            break;
                        case ActionType.TempBan:
                        case ActionType.Ban:
                        case ActionType.HackBan:
                            var bans = await guild.GetBansAsync();
                            if (bans.Any(x => x.User.Id == guildCase.TargetId))
                                await guild.RemoveBanAsync(guildCase.TargetId);
                            await UpdateDiscordMessage(guildCase);
                            await dbService.CreateNewCaseAsync(guild, (reason == null ? $"``Responsible moderator please do {config.Prefix} reason {{0}} <reason>``" : reason) + (guildCase.ActionType == ActionType.TempBan ? (guildCase.GetDiscordMessageLink() == null ? "" : (guildCase.Guild.LogStyle == LogStyle.Basic ? $" (Tied to Case #{guildCase.Id})" : $"(Tied to [Case #{guildCase.Id}]({guildCase.GetDiscordMessageLink()}))")) : ""), ActionType.Unban, 0, resolver.Id, guildCase.TargetId, doLock);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
                await Resolve(guildCase.GuildId, guildCase.Id, false, force, doLock);
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
                if (modlogChannel == null)
                    throw new NullReferenceException("Log channel has been deleted.");

                var modlogPerms = discordService.discordClient.GetGuild(dbguild.Id).CurrentUser.GetPermissions(modlogChannel);

                if (!modlogPerms.ViewChannel || !modlogPerms.SendMessages || !modlogPerms.EmbedLinks)
                    throw new InvalidOperationException("Don't have enough permissions.");

                var msg = await modlogChannel.GetMessageAsync(guildCase.DiscordMessageId) as IUserMessage;
                switch (guildCase.Guild.LogStyle)
                {
                    case LogStyle.Basic:
                        await msg.ModifyAsync(async x => x.Content = await helperService.ConstructCaseMessageAsync(guildCase));
                        break;
                    case LogStyle.Modern:
                        var eb = new EmbedBuilder();
                        eb.WithTitle($"Case **{guildCase.Id}**  »  {guildCase.ActionType.Humanize()}");
                        eb.WithDescription(await helperService.ConstructCaseMessageAsync(guildCase));
                        await msg.ModifyAsync(x => { x.Content = null; x.Embed = eb.Build(); });
                        break;
                    default:
                        break;
                }
            }
        }


        private async Task Resolve(ulong guildId, int caseId, bool isInmemory, bool force, bool doLock = true)
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

                var currentUser = guild.CurrentUser;

                if (!currentUser.GuildPermissions.BanMembers || !currentUser.GuildPermissions.ManageRoles || !currentUser.GuildPermissions.KickMembers || !currentUser.GuildPermissions.ManageChannels)
                    throw new InvalidOperationException("Don't have enough permissions.");


                var dbService = _serviceProvider.GetService<DatabaseService>();
                var helperService = _serviceProvider.GetService<HelperService>();


                var dbguild = await guildStorage.GetOrCreateGuildAsync(guildId);

                var guildCase = dbguild.Cases.FirstOrDefault(x => x.Id == caseId);

                guildCase.Resolved = true;
                if (force)
                    guildCase.ForceResolved = true;

                await dbService.UpdateCaseAsync(guildCase, doLock);

                switch (guildCase.ActionType)
                {
                    case ActionType.Warn:
                        await UpdateDiscordMessage(guildCase);
                        await dbService.CreateNewCaseAsync(guild, "Warn expired. " + (guildCase.GetDiscordMessageLink() == null ? "" : (guildCase.Guild.LogStyle == LogStyle.Basic ? $" (Tied to Case #{guildCase.Id})" : $"(Tied to [Case #{guildCase.Id}]({guildCase.GetDiscordMessageLink()}))")), ActionType.Unwarn, 0, discordService.discordClient.CurrentUser.Id, guildCase.TargetId, doLock);
                        break;
                    case ActionType.TempMute:
                        await UpdateDiscordMessage(guildCase);
                        IRole muteRole = guild.GetRole(dbguild.MuteRoleId);
                        if (muteRole != null)
                        {
                            var target = guild.GetUser(guildCase.TargetId);
                            if (target != null)
                                await target.RemoveRoleAsync(muteRole);
                        }

                        await dbService.CreateNewCaseAsync(guild, "Temporary mute expired. " + (guildCase.GetDiscordMessageLink() == null ? "" : (guildCase.Guild.LogStyle == LogStyle.Basic ? $" (Tied to Case #{guildCase.Id})" : $"(Tied to [Case #{guildCase.Id}]({guildCase.GetDiscordMessageLink()}))")), ActionType.Unmute, 0, discordService.discordClient.CurrentUser.Id, guildCase.TargetId, doLock);
                        break;
                    case ActionType.TempBan:
                        await UpdateDiscordMessage(guildCase);
                        await dbService.CreateNewCaseAsync(guild, "Temporary ban expired. " + (guildCase.GetDiscordMessageLink() == null ? "" : (guildCase.Guild.LogStyle == LogStyle.Basic ? $" (Tied to Case #{guildCase.Id})" : $"(Tied to [Case #{guildCase.Id}]({guildCase.GetDiscordMessageLink()}))")), ActionType.Unban, 0, discordService.discordClient.CurrentUser.Id, guildCase.TargetId, doLock);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
