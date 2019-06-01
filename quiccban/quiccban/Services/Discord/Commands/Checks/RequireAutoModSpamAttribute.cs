using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace quiccban.Services.Discord.Commands
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireAutoModSpamAttribute : CheckBaseAttribute
    {

        public override async Task<CheckResult> CheckAsync(ICommandContext ctx, IServiceProvider services)
        {
            var context = (QuiccbanContext)ctx;

            var dbService = services.GetService<DatabaseService>();
            var responseService = services.GetService<ResponseService>();

            var dbGuild = await dbService.GetOrCreateGuildAsync(context.Guild);

            return (dbGuild.AutoMod.SpamEnabled ? new CheckResult(responseService.Get("require_spam_automod_for_modification")) : CheckResult.Successful);
        }
    }
}
