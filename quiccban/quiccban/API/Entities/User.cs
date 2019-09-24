using Discord;
using quiccban.Services.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API.Entities
{
    public class User
    {
        public User(IUser user)
        {
            Id = user.Id.ToString();
            Username = user.Username;
            Discriminator = user.Discriminator;
            AvatarId = user.AvatarId;
            AvatarUrl = $"{DiscordConfig.CDNUrl}avatars/{Id}/{AvatarId}" ?? user.GetDefaultAvatarUrl();
            IsBotOwner = DiscordService.ApplicationInfo.Owner.Id == user.Id;
            CreatedAt = user.CreatedAt;
        }

        public string Id;
        public string Username;
        public string Discriminator;
        public string AvatarId;
        public string AvatarUrl;
        public bool IsBotOwner;
        public DateTimeOffset CreatedAt;
    }
}
