using Qmmands;
using quiccban.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace quiccban.Services.Discord.Commands
{
    public sealed class CaseParser : TypeParser<Case>
    {
        public async override Task<TypeParserResult<Case>> ParseAsync(Parameter parameter, string value, ICommandContext ctx, IServiceProvider provider)
        {
            var context = (QuiccbanContext)ctx;


            var databaseService = provider.GetService<DatabaseService>();

            var dbGuild = await databaseService.GetOrCreateGuildAsync(context.Guild);

            Case @case = null;
            if (value == "|")
                @case = dbGuild.Cases.LastOrDefault();

            if (@case == null)
            {
                if (int.TryParse(value, out int id))
                    @case = dbGuild.Cases.FirstOrDefault(x => x.Id == id);
            }

            return @case == null
                ? new TypeParserResult<Case>("No case found.")
                : new TypeParserResult<Case>(@case);

        }
    }
}
