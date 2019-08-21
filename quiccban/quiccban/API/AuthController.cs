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

namespace quiccban.API
{
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        OAuthCachingService _oAuthCaching;

        public AuthController(OAuthCachingService oAuthCaching)
        {
            _oAuthCaching = oAuthCaching;
        }


        [HttpGet("login")]
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, DiscordAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> LogOut()
        {
            _oAuthCaching.Remove(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "accessToken").Value);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);


            return Redirect("/");
        }

       
    }
}
