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

namespace quiccban.Services
{
    public class DatabaseService
    {
        private CaseHandlingService _caseHandlingService;
        private readonly SemaphoreSlim databaseLock = new SemaphoreSlim(1, 1);

        public DatabaseService(IServiceProvider provider)
        {
            _caseHandlingService = provider.GetService<CaseHandlingService>();
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

        public async Task<Case> CreateNewCaseAsync(IGuild guild, string reason, ActionType actionType, ulong actionExpiry, ulong issuerId, ulong targetId)
        {
            await databaseLock.WaitAsync();
            try
            {
                using (var guildStorage = new GuildStorage())
                {
                    var dbGuild = await guildStorage.GetOrCreateGuildAsync(guild.Id);

                    Case @case = new Case
                    {
                        Reason = reason,
                        ActionType = actionType,
                        IssuerId = issuerId,
                        TargetId = targetId,
                        ActionExpiry = actionExpiry,
                        UnixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };

                    switch (actionType)
                    {
                        case ActionType.Warn:
                        case ActionType.Tempmute:
                        case ActionType.Tempban:
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

                    _caseHandlingService.TryAdd(latestCase);

                    return latestCase;

                }
            }
            finally
            {
                databaseLock.Release();
            }
        }

        public async Task ResolveCaseAsync(Case guildCase)
        {
            await databaseLock.WaitAsync();
            try
            {
                await _caseHandlingService.ResolveAsync(guildCase);
            }
            finally
            {
                databaseLock.Release();
            }
        }

    }
}
