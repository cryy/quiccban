using Qmmands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.Services.Discord.Commands
{
    public class QuiccbanContext : ICommandContext
    {
        public DiscordSocketClient Client { get; }
        public SocketGuild Guild { get; }
        public SocketTextChannel Channel { get; }
        public SocketGuildUser User { get; }
        public SocketUserMessage Message { get; }
        public IServiceProvider Services { get; }

        public QuiccbanContext(DiscordSocketClient client, SocketUserMessage msg, IServiceProvider services)
        {
            Client = client;
            Channel = msg.Channel as SocketTextChannel;
            Guild = Channel.Guild;
            User = msg.Author as SocketGuildUser;
            Message = msg;
            Services = services;
        }
    }
}
