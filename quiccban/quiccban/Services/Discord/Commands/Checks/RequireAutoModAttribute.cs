using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace quiccban.Services.Discord.Commands.Checks
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireAutoModAttribute : CheckBaseAttribute
    {
        public override async Task<CheckResult> CheckAsync(ICommandContext ctx, IServiceProvider services)
        {
            var context = (QuiccbanContext)ctx;

            var dbService = services.GetService<DatabaseService>();
            var responseService = services.GetService<ResponseService>();

            var dbGuild = await dbService.GetOrCreateGuildAsync(context.Guild);

            return (dbGuild.AutoMod.Enabled ? new CheckResult(responseService.Get("require_automod_for_modification")) : CheckResult.Successful);
        }
    }
}
