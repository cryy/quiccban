using Microsoft.AspNetCore.SignalR;
using quiccban.Services.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API
{
    public class SocketHub : Hub
    {
        private Config _config;
        private DiscordService _discord;

        public SocketHub(Config config, DiscordService discord)
        {
            _config = config;
            _discord = discord;
        }

        public override async Task OnConnectedAsync()
        {

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
