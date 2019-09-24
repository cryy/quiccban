using Discord;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API.Entities
{
    public class SelfUser
    {
        public SelfUser(RestSelfUser user, IEnumerable<RestUserGuild> guilds)
        {
            User = new User(user);
            Guilds = guilds;
        }

        public User User;
        public IEnumerable<RestUserGuild> Guilds;
    }
}
