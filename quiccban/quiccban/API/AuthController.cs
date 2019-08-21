using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using quiccban.API.Entities;
using quiccban.Services;
using quiccban.Services.Discord;

namespace quiccban.API
{
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        OAuthCachingService _oAuthCaching;
        DiscordService _discordService;

        public AuthController(OAuthCachingService oAuthCaching, DiscordService discordService)
        {
            _oAuthCaching = oAuthCaching;
            _discordService = discordService;
        }


        [HttpGet("login")]
        public IActionResult Login()
        {
            if (!_discordService.IsReady)
                return StatusCode(503, "Discord client is not ready yet.");

            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, DiscordAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> LogOut()
        {
            if (!_discordService.IsReady)
                return StatusCode(503, "Discord client is not ready yet.");

            _oAuthCaching.Remove(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "accessToken").Value);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);


            return Redirect("/");
        }

       
    }
}
