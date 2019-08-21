using Discord;
using quiccban.Services.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API.Entities
{
    public struct User
    {
        public User(IUser user)
        {
            Id = user.Id.ToString();
            Username = user.Username;
            Discriminator = user.DiscriminatorValue;
            AvatarId = user.AvatarId;
            AvatarUrl = $"{DiscordConfig.CDNUrl}avatars/{Id}/{AvatarId}" ?? user.GetDefaultAvatarUrl();
            IsBotOwner = DiscordService.ApplicationInfo.Owner.Id.ToString() == Id;
            CreatedAt = user.CreatedAt;
        }

        public string Id;
        public string Username;
        public ushort Discriminator;
        public string AvatarId;
        public string AvatarUrl;
        public bool IsBotOwner;
        public DateTimeOffset CreatedAt;
    }
}
