using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API.Entities
{
    public class SelfGuild
    {
        public SelfGuild(IUserGuild userGuild)
        {

        }

        public string Name;
        public bool IsOwner;
        public int Permissions;

    }
}
