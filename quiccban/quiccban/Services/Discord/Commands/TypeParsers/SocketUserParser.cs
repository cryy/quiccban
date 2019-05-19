using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quiccban.Services.Discord.Commands
{
    public sealed class SocketUserParser<TUser> : TypeParser<TUser> where TUser : SocketUser
    {
        public override Task<TypeParserResult<TUser>> ParseAsync(Parameter parameter, string value, ICommandContext ctx, IServiceProvider provider)
        {
            var context = (QuiccbanContext)ctx;
            var member = false;
            IEnumerable<TUser> users;
            var type = typeof(TUser);
            if (typeof(SocketGuildUser).IsAssignableFrom(type))
            {
                if (context.Guild == null)
                    return Task.FromResult(new TypeParserResult<TUser>("This command must be used in a guild."));

                member = true;
                users = context.Guild.Users.OfType<TUser>();
            }
            else if (context.Guild == null)
                users = (context.Channel as ISocketPrivateChannel).Recipients.OfType<TUser>();
            else
                users = context.Guild.Users.OfType<TUser>();

            TUser user = null;
            if (value.Length > 3 && value[0] == '<' && value[1] == '@' && value[value.Length - 1] == '>' && ulong.TryParse(value[2] == '!' ? value.Substring(3, value.Length - 4) : value.Substring(2, value.Length - 3), out var id)
                || ulong.TryParse(value, out id))
                user = users.FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                var hashIndex = value.LastIndexOf('#');
                if (hashIndex != -1 && hashIndex + 5 == value.Length)
                    user = users.FirstOrDefault(x => x.Username == value.Substring(0, value.Length - 5) && x.Discriminator == value.Substring(hashIndex + 1));
            }

            if (user == null)
            {
                IReadOnlyList<TUser> matchingUsers;
                if (context.Guild != null)
                    matchingUsers = users.Where(x => x.Username == value || (x as SocketGuildUser).Nickname == value).ToImmutableArray();
                else
                    matchingUsers = users.Where(x => x.Username == value).ToImmutableArray();

                if (matchingUsers.Count > 1)
                    return Task.FromResult(new TypeParserResult<TUser>($"Multiple matches found. Mention the {(member ? "member" : "user")} or use their ID."));

                if (matchingUsers.Count == 1)
                    user = matchingUsers[0];
            }

            return user == null
                ? Task.FromResult(new TypeParserResult<TUser>($"No {(member ? "member" : "user")} found matching the input."))
                : Task.FromResult(new TypeParserResult<TUser>(user));
        }
    }
}
