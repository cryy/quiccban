using Discord;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace quiccban.Services.Discord.Commands
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireOwnerAttribute : CheckBaseAttribute
    {
        public override async Task<CheckResult> CheckAsync(ICommandContext ctx, IServiceProvider services)
        {
            var context = (QuiccbanContext)ctx;

            var responseService = services.GetService<ResponseService>();

            var application = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
            if (context.User.Id != application.Owner.Id)
                return new CheckResult(responseService.Get("owner_only_command"));
            return CheckResult.Successful;

        }
    }
}
