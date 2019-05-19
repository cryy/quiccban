using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quiccban.Services.Discord.Commands
{
    public sealed class SocketRoleParser : TypeParser<SocketRole>
    {
        public override Task<TypeParserResult<SocketRole>> ParseAsync(Parameter parameter, string value, ICommandContext ctx, IServiceProvider provider)
        {
            var context = (QuiccbanContext)ctx;
            if (context.Guild == null)
                return Task.FromResult(new TypeParserResult<SocketRole>("This command must be used in a guild."));

            SocketRole role = null;
            if (value.Length > 3 && value[0] == '<' && value[1] == '@' && value[2] == '&' && value[value.Length - 1] == '>' && ulong.TryParse(value.Substring(3, value.Length - 4), out var id)
                || ulong.TryParse(value, out id))
                role = context.Guild.Roles.FirstOrDefault(x => x.Id == id);

            if (role == null)
                role = context.Guild.Roles.FirstOrDefault(x => x.Name == value);

            return role == null
                ? Task.FromResult(new TypeParserResult<SocketRole>("No role found matching the input."))
                : Task.FromResult(new TypeParserResult<SocketRole>(role));
        }
    }
}
