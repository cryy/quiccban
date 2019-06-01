using Discord;
using Discord.Rest;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quiccban.Services.Discord.Commands
{
    public sealed class BanParser : TypeParser<RestBan>
    {
        public override async Task<TypeParserResult<RestBan>> ParseAsync(Parameter parameter, string value, ICommandContext ctx, IServiceProvider provider)
        {
            var context = (QuiccbanContext)ctx;
            IEnumerable<RestBan> bans;
            RestBan ban = null;

            bans = (await context.Guild.GetBansAsync()).OfType<RestBan>();

            if (value.Length > 3 && value[0] == '<' && value[1] == '@' && value[value.Length - 1] == '>' && ulong.TryParse(value[2] == '!' ? value.Substring(3, value.Length - 4) : value.Substring(2, value.Length - 3), out var id)
                || ulong.TryParse(value, out id))
                ban = bans.FirstOrDefault(x => x.User.Id == id);

            if (ban == null)
            {
                var hashIndex = value.LastIndexOf('#');
                if (hashIndex != -1 && hashIndex + 5 == value.Length)
                    ban = bans.FirstOrDefault(x => x.User.Username == value.Substring(0, value.Length - 5) && x.User.Discriminator == value.Substring(hashIndex + 1));
            }

            if (ban == null)
            {
                IReadOnlyList<RestBan> matchingBans;

                matchingBans = bans.Where(x => x.User.Username == value).ToImmutableArray();

                if (matchingBans.Count > 1)
                    return new TypeParserResult<RestBan>($"Multiple matches found. Use the user's ID or username#discriminator.");

                if (matchingBans.Count == 1)
                    ban = matchingBans[0];
            }

            return ban == null
                ? new TypeParserResult<RestBan>($"No user found matching the input.")
                : new TypeParserResult<RestBan>(ban);
        }
    }
}
