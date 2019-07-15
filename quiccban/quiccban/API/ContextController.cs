using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public ContextController(DiscordService discordService)
        {
            _discordService = discordService;
        }

        [HttpGet("user")]
        public async Task<IActionResult> Userinfo()
        {
            if (User.Identity.IsAuthenticated)
            {
                var claims = User.Claims;

                return Ok(await User.Claims.ToSelfUserAsync(_discordService));

            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
