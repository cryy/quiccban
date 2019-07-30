using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API.Entities
{
    public struct SelfUser
    {
        public string Id;
        public string Username;
        public ushort Discriminator;
        public string AvatarHash;
        public ushort Flags;
        public ushort? PremiumType;
        public bool IsBotOwner;
    }
}
