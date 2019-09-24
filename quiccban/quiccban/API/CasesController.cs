using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using quiccban.API.Entities;
using quiccban.API.Filters;
using quiccban.Database.Models;
using quiccban.Services;
using quiccban.Services.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API
{
    [Route("api/cases")]
    [ServiceFilter(typeof(RequireAuthAttribute))]
    [ServiceFilter(typeof(RequireReadyClientAttribute))]
    public class CasesController : ControllerBase
    {
        DiscordService _discordService;
        OAuthCachingService _oAuthCaching;
        DatabaseService _databaseService;
        public CasesController(DiscordService discordService, OAuthCachingService oAuthCaching, DatabaseService databaseService)
        {
            _discordService = discordService;
            _oAuthCaching = oAuthCaching;
            _databaseService = databaseService;
        }

        [HttpGet("recent")]
        public async Task<IActionResult> RecentCases()
        {
            var guilds = (IEnumerable<RestUserGuild>)HttpContext.Items["guilds"];

            var dbguilds = await _databaseService.GetAllGuildsAsync();
            var recentCases = dbguilds.Where(x => guilds.Any(y => x.Id == y.Id)).SelectMany(x => x.Cases).OrderByDescending(x => x.UnixTimestamp).Take(10);

            var apiCases = new HashSet<APICase>();
            foreach(var recentCase in recentCases)
            {
                var guild = _discordService.discordClient.GetGuild(recentCase.GuildId);
                User targetUser = null;
                User issuerUser = null;

                targetUser = guild == null ? new User(await _discordService.discordClient.Rest.GetUserAsync(recentCase.TargetId)) : new User(guild.GetUser(recentCase.TargetId) ?? (IUser)await _discordService.discordClient.Rest.GetUserAsync(recentCase.TargetId));
                issuerUser = guild == null ? new User(await _discordService.discordClient.Rest.GetUserAsync(recentCase.TargetId)) : new User(guild.GetUser(recentCase.IssuerId) ?? (IUser)await _discordService.discordClient.Rest.GetUserAsync(recentCase.IssuerId));

                apiCases.Add(new APICase(recentCase, targetUser, issuerUser));
            }


            return Ok(apiCases);
        }
    }
}
