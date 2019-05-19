using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban
{
    public class Config
    {
        public string DiscordToken { get; }
        
        public string Prefix { get; }

        public bool AllowMentionPrefix { get; }

        public Config(string token, string prefix, bool mentionPrefix)
        {
            DiscordToken = token;
            Prefix = prefix;
            AllowMentionPrefix = mentionPrefix;
        }


    }
}
