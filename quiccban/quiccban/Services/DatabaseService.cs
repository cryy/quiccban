using Discord;
using quiccban.Database;
using quiccban.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ActionType = quiccban.Database.Models.ActionType;
using System.Threading;
using quiccban.Services.Discord;
using Discord.WebSocket;
using Humanizer;

namespace quiccban.Services
{
    public class DatabaseService
    {
        private CaseHandlingService _caseHandlingService;
        private HelperService _helperService;
        private IServiceProvider _serviceProvider;
        private readonly SemaphoreSlim databaseLock = new SemaphoreSlim(1, 1);

        public DatabaseService(IServiceProvider provider)
        {
            _caseHandlingService = provider.GetService<CaseHandlingService>();
            _helperService = provider.GetService<HelperService>();
            _serviceProvider = provider;
        }

        public Task<Guild> GetOrCreateGuildAsync(IGuild guild)
            => GetOrCreateGuildAsync(guild.Id);
        public async Task<Guild> GetOrCreateGuildAsync(ulong guildId)
        {
            await databaseLock.WaitAsync();
            try
            {
                using (var guildStorage = new GuildStorage())
                {
                    return await guildStorage.GetOrCreateGuildAsync(guildId);
                }
            }
            finally
            {
                databaseLock.Release();
            }
        }

        public async Task<List<Guild>> GetAllGuildsAsync()
        {
            await databaseLock.WaitAsync();
            try
            {
                using (var guildStorage = new GuildStorage())
                {
                    return await guildStorage.GetAllGuildsAsync();
                }
            }
            finally
            {
                databaseLock.Release();
            }
        }

        public async Task<Case> CreateNewCaseAsync(IGuild guild, string reason, ActionType actionType, int actionExpiry, ulong issuerId, ulong targetId, bool doLock = true)
        {
            if(doLock)
            await databaseLock.WaitAsync();
            try
            {
                var currentUser = await guild.GetCurrentUserAsync();

                if (!currentUser.GuildPermissions.BanMembers || !currentUser.GuildPermissions.ManageRoles || !currentUser.GuildPermissions.KickMembers || !currentUser.GuildPermissions.ManageChannels)
                    throw new InvalidOperationException("Don't have enough permissions.");

                using (var guildStorage = new GuildStorage())
                {
                    var dbGuild = await guildStorage.GetOrCreateGuildAsync(guild.Id);

                    if (dbGuild.ModlogChannelId == 0)
                        throw new InvalidOperationException("Can't create a new case without a log channel.");

                    Case @case = new Case
                    {
                        Reason = reason,
                        ActionType = actionType,
                        IssuerId = issuerId,
                        TargetId = targetId,
                        ActionExpiry = actionExpiry,
                        UnixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        ForceResolved = false,
                        Id = dbGuild.Cases.LastOrDefault() == null ? 1 : dbGuild.Cases.LastOrDefault().Id + 1
                    };

                    switch(actionType)
                    {
                        case ActionType.Warn:
                        case ActionType.TempBan:
                        case ActionType.TempMute:
                            @case.Resolved = false;
                            break;
                        default:
                            @case.Resolved = true;
                            break;
                    }

                    dbGuild.Cases.Add(@case);

                    guildStorage.Update(dbGuild);

                    await guildStorage.SaveChangesAsync();

                    var latestCase = dbGuild.Cases.LastOrDefault();

                    var discordService = _serviceProvider.GetService<DiscordService>();

                    var logChannel = discordService.discordClient.GetChannel(dbGuild.ModlogChannelId) as SocketTextChannel;

                    if(logChannel == null)
                        throw new InvalidOperationException("Can't log to a channel that doesn't exist");

                    IUserMessage msg = null;
                    switch (dbGuild.LogStyle)
                    {
                        case LogStyle.Basic:
                            msg = await logChannel.SendMessageAsync(await _helperService.ConstructCaseMessageAsync(latestCase));
                            break;
                        case LogStyle.Modern:
                            var eb = new EmbedBuilder();
                            eb.WithTitle($"Case **{latestCase.Id}**  »  {latestCase.ActionType.Humanize()}");
                            eb.WithDescription(await _helperService.ConstructCaseMessageAsync(latestCase));
                            msg = await logChannel.SendMessageAsync(embed: eb.Build());
                            break;
                    }

                    latestCase.DiscordMessageId = msg.Id;

                    guildStorage.Update(dbGuild);

                    await guildStorage.SaveChangesAsync();

                    if (latestCase.ActionType == ActionType.Warn || latestCase.ActionExpiry > 0)
                    _caseHandlingService.TryAdd(latestCase);

                    if (actionType == ActionType.Warn)
                    {
                        if (dbGuild.Cases.Count(x => !x.Resolved && x.ActionType == ActionType.Warn && x.TargetId == targetId) > dbGuild.WarnThreshold)
                        {
                            if (!dbGuild.Cases.Any(x => !x.Resolved && x.ActionType == dbGuild.WarnThresholdActionType))
                            {
                                _ = Task.Delay(100).ContinueWith(async _ =>
                                {
                                    await databaseLock.WaitAsync();
                                    try
                                    {
                                        currentUser = await guild.GetCurrentUserAsync();

                                        if (!currentUser.GuildPermissions.BanMembers || !currentUser.GuildPermissions.ManageRoles || !currentUser.GuildPermissions.KickMembers || !currentUser.GuildPermissions.ManageChannels)
                                            throw new InvalidOperationException("Don't have enough permissions.");

                                        var socketguild = discordService.discordClient.GetGuild(guild.Id);
                                        var user = socketguild.GetUser(targetId);

                                        switch (dbGuild.WarnThresholdActionType)
                                        {
                                            case ActionType.Mute:
                                            case ActionType.TempMute:
                                                IRole muteRole = socketguild.GetRole(dbGuild.MuteRoleId);
                                                if (muteRole == null)
                                                    muteRole = await _helperService.CreateMuteRoleAsync(dbGuild, false);

                                                await user.AddRoleAsync(muteRole);
                                                break;
                                            case ActionType.Ban:
                                            case ActionType.TempBan:
                                                await user.BanAsync(0, "Warn threshold crossed.");
                                                break;
                                            case ActionType.Kick:
                                                await user.KickAsync("Warn threshold crossed.");
                                                break;
                                            default:
                                                break;
                                        }

                                        await CreateNewCaseAsync(guild, "Warn threshold crossed.", dbGuild.WarnThresholdActionType, dbGuild.WarnThresholdActionExpiry, discordService.discordClient.CurrentUser.Id, targetId, false);
                                    }
                                    finally
                                    {
                                        databaseLock.Release();
                                    }

                                });
                            }
                        }
                    }

                    return latestCase;

                }
            }
            finally
            {
                if(doLock)
                databaseLock.Release();
            }
        }

        public async Task ResolveCaseAsync(Case guildCase, IUser resolver, string reason)
        {
            await databaseLock.WaitAsync();
            try
            {
                await _caseHandlingService.ResolveAsync(guildCase, resolver, reason, true, false);
            }
            finally
            {
                databaseLock.Release();
            }
        }

        public async Task UpdateCaseAsync(Case @case, bool doLock)
        {
            if(doLock)
            await databaseLock.WaitAsync();
            try
            {
                using (var guildStorage = new GuildStorage())
                {

                    var guildCase = @case.Guild.Cases.FirstOrDefault(x => x.Id == @case.Id);

                    guildCase = @case;

                    guildStorage.Update(@case.Guild);

                    await guildStorage.SaveChangesAsync();
                }

            }
            finally
            {
                if(doLock)
                databaseLock.Release();
            }
        }

        public async Task UpdateGuildAsync(Guild guild, bool doLock = true)
        {
            if(doLock)
            await databaseLock.WaitAsync();
            try
            {
                using (var guildStorage = new GuildStorage())
                {
                    guildStorage.Update(guild);

                    await guildStorage.SaveChangesAsync();
                }

            }
            finally
            {
                if(doLock)
                databaseLock.Release();
            }
        }


    }
}
