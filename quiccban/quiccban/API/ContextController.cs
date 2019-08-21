using Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using quiccban.API.Entities;
using quiccban.Services;
using quiccban.Services.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API
{
    [Route("api/context")]
    public class ContextController : ControllerBase
    {
        DiscordService _discordService;
        OAuthCachingService _oAuthCaching;
        public ContextController(DiscordService discordService, OAuthCachingService oAuthCaching)
        {
            _discordService = discordService;
            _oAuthCaching = oAuthCaching;
        }

        [HttpGet("user")]
        public async Task<IActionResult> Userinfo()
        {
            if (!_discordService.IsReady)
                return StatusCode(503, "Discord client is not ready yet.");

            if (User.Identity.IsAuthenticated)
            {

                var client = await _oAuthCaching.GetOrCreateClient(User.Claims.FirstOrDefault(x => x.Type == "accessToken").Value);
                var guilds = await client.GetGuildSummariesAsync().FlattenAsync();

                return Ok(new SelfUser(client.CurrentUser, guilds));

            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
