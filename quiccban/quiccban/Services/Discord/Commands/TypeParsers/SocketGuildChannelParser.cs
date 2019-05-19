using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quiccban.Services.Discord.Commands
{
    public sealed class SocketGuildChannelParser<TChannel> : TypeParser<TChannel> where TChannel : SocketGuildChannel
    {
        public override Task<TypeParserResult<TChannel>> ParseAsync(Parameter parameter, string value, ICommandContext ctx, IServiceProvider service)
        {
            var context = (QuiccbanContext)ctx;
            if (context.Guild == null)
                return Task.FromResult(new TypeParserResult<TChannel>("This command must be used in a guild."));

            var text = false;
            IEnumerable<TChannel> channels;
            var type = typeof(TChannel);
            if (typeof(SocketTextChannel).IsAssignableFrom(type))
            {
                text = true;
                channels = context.Guild.TextChannels.OfType<TChannel>();
            }
            else if (typeof(SocketVoiceChannel).IsAssignableFrom(type))
                channels = context.Guild.VoiceChannels.OfType<TChannel>();
            else if (typeof(SocketCategoryChannel).IsAssignableFrom(type))
                channels = context.Guild.CategoryChannels.OfType<TChannel>();
            else
                channels = context.Guild.Channels.OfType<TChannel>();

            TChannel channel = null;
            if (value.Length > 3 && value[0] == '<' && value[1] == '#' && value[value.Length - 1] == '>' && ulong.TryParse(value.Substring(2, value.Length - 3), out var id)
                || ulong.TryParse(value, out id))
                channel = channels.FirstOrDefault(x => x.Id == id);

            if (channel == null)
                channel = channels.FirstOrDefault(x => x.Name == value);

            if (channel == null && text && value.StartsWith('#'))
                channel = channels.FirstOrDefault(x => x.Name == value.Substring(1));

            return channel == null
                ? Task.FromResult(new TypeParserResult<TChannel>("No channel found."))
                : Task.FromResult(new TypeParserResult<TChannel>(channel));
        }
    }
}
