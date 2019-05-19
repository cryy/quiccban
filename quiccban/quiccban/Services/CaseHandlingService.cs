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
            var cts = new CancellationTokenSource();

            if (!InmemoryExpiringCases.TryAdd(guildCase, cts))
            {
                cts.Dispose();
                return false;
            }
            
            
            Task.Run(async () =>
            {
                try
                {
                    var ms = (int)(guildCase.GetEndingTime() - DateTimeOffset.UtcNow).TotalMilliseconds;
                    await Task.Delay(ms, cts.Token);
                }
                catch { }
                await Resolve(guildCase.GuildId, guildCase.Id, true);
            });

            return true;
        }

        public async Task ResolveAsync(Case guildCase)
        {
            var c = InmemoryExpiringCases.FirstOrDefault(x => x.Key.Id == guildCase.Id && x.Key.GuildId == guildCase.GuildId);
            if(c.Key == null)
            {
                await Resolve(guildCase.GuildId, guildCase.Id, false);
            }
            else
                c.Value.Cancel();
        }

        private async Task Resolve(ulong guildId, int caseId, bool isInmemory)
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

                var dbguild = await guildStorage.GetOrCreateGuildAsync(guildId);

                var guildCase = dbguild.Cases.FirstOrDefault(x => x.Id == caseId);


                switch(guildCase.ActionType)
                {
                    case ActionType.Warn:
                        break;
                    case ActionType.Tempmute:
                        break;
                    case ActionType.Tempban:
                        break;
                    default:
                        break;
                }

                guildCase.Resolved = true;

                guildStorage.Update(dbguild);

                await guildStorage.SaveChangesAsync();
            }
        }
    }
}
