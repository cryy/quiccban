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
        public string Message { get; }

        public RequireAutoModAttribute(string message)
        {
            Message = message;
        }

        public override async Task<CheckResult> CheckAsync(ICommandContext ctx, IServiceProvider services)
        {
            var context = (QuiccbanContext)ctx;

            var dbService = services.GetService<DatabaseService>();

            var dbGuild = await dbService.GetOrCreateGuildAsync(context.Guild);

            return (dbGuild.AutoMod.Enabled ? new CheckResult(Message) : CheckResult.Successful);
        }
    }
}
