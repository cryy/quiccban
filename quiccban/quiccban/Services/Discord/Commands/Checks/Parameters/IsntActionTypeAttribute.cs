using Qmmands;
using quiccban.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace quiccban.Services.Discord.Commands
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class IsntActionTypeAttribute : ParameterCheckBaseAttribute
    {
        public ActionType[] Types;

        public IsntActionTypeAttribute(params ActionType[] types)
        {
            Types = types;
        }

        public override Task<CheckResult> CheckAsync(object argument, ICommandContext ctx, IServiceProvider provider)
        {
            var context = (QuiccbanContext)ctx;

            if (!(argument is ActionType type))
                throw new InvalidCastException("Parameter isn't ActionType.");

            var responseService = provider.GetService<ResponseService>();

            return Types.Any(x => x == type) ? Task.FromResult(new CheckResult(responseService.Get("value_not_allowed"))) : Task.FromResult(CheckResult.Successful);
        }
    }
}
