using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using quiccban.API.Entities;
using quiccban.API.Filters;
using quiccban.Services;
using quiccban.Services.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API
{
    [Route("api/context")]
    [ServiceFilter(typeof(RequireAuthAttribute))]
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
        public IActionResult Userinfo()
        {
            var client = (DiscordRestClient)HttpContext.Items["client"];
            var guilds = (IEnumerable<RestUserGuild>)HttpContext.Items["guilds"];

            return Ok(new SelfUser(client.CurrentUser, guilds));
        }
    }
}
