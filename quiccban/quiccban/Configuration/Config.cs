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
        public bool UseWebUI { get; }
        public string ClientSecret { get; }
        public ulong? ClientId { get; }

        public Config(string token, string prefix, bool mentionPrefix, bool useWeb, string clientSecret, ulong? clientId)
        {
            DiscordToken = token;
            Prefix = prefix;
            AllowMentionPrefix = mentionPrefix;
            UseWebUI = useWeb;
            ClientSecret = clientSecret;
            ClientId = clientId;
        }


    }
}
