using quiccban.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API.Entities
{
    public class DatabaseGuild : PartialDatabaseGuild
    {
        public DatabaseGuild(Guild guild) : base(guild)
        {
            Cases = guild.Cases.Select(x => new APICase(x));
        }

        public IEnumerable<APICase> Cases;
    }
}
