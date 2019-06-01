using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace quiccban.Services.Discord.Commands
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class RequireHigherBotHierarchyAttribute : ParameterCheckBaseAttribute
    {
        public override Task<CheckResult> CheckAsync(object argument, ICommandContext ctx, IServiceProvider provider)
        {
            var context = (QuiccbanContext)ctx;

            if (!(argument is SocketGuildUser u))
                throw new InvalidCastException("Parameter isn't a guild user.");

            var responseService = provider.GetService<ResponseService>();

            return context.Guild.CurrentUser.Hierarchy > u.Hierarchy ? Task.FromResult(CheckResult.Successful) : Task.FromResult(new CheckResult(responseService.Get("bot_require_higher_hierarchy")));
        }
    }
}
