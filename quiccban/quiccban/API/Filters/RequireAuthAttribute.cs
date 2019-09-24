using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using quiccban.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API.Filters
{
    public class RequireAuthAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        OAuthCachingService _oAuthCaching;

        public RequireAuthAttribute(OAuthCachingService oAuthCaching)
        {
            _oAuthCaching = oAuthCaching;
        }


        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                IEnumerable<RestUserGuild> guilds;
                try
                {
                    var client = await _oAuthCaching.GetOrCreateClient(context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "accessToken").Value);
                    guilds = await client.GetGuildSummariesAsync().FlattenAsync();

                    context.HttpContext.Items.Add("guilds", guilds);
                    context.HttpContext.Items.Add("client", client);
                }
                catch
                {
                    context.Result = new LocalRedirectResult("~/api/auth/logout");
                }

            }
            else
            {
                context.Result = new UnauthorizedObjectResult(new { Code = 401, Message = "Unauthorized." });
            }
        }
    }
}
