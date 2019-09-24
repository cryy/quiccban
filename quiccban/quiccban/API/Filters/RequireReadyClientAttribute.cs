using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using quiccban.Services.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API.Filters
{
    public class RequireReadyClientAttribute : IActionFilter
    {
        DiscordService _discordService;

        public RequireReadyClientAttribute(DiscordService discordService)
        {
            _discordService = discordService;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!_discordService.IsReady)
            {
                context.Result = new ObjectResult(new { Code = 503, Message = "Discord client is not ready yet." }) { StatusCode = 503 };
                return;
            }
        }
    }
}
