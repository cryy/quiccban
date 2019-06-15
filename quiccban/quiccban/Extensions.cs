using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban
{
    public static class Extensions
    {

        private static string[] RequiredConfigValues = new string[]
        {
            "discordToken",
            "prefix",
            "allowMentionPrefix",
            "useOAuth"
        };


        public static ConfigResult ParseConfig(this JObject config)
        {
           

            if (!RequiredConfigValues.All(x => config.Properties().Any(y => y.Name == x)))
                return new ConfigResult { IsValid = false, Message = "Required config values are missing." };

            if (RequiredConfigValues.Any(x => string.IsNullOrWhiteSpace(config.Properties().First(y => y.Name == x).Value.ToString())))
                return new ConfigResult { IsValid = false, Message = "Required config values are null or empty." };

            if (!bool.TryParse(config.GetValue("allowMentionPrefix").Value<string>(), out bool allowMentionPrefix))
                return new ConfigResult { IsValid = false, Message = "\"allowMentionPrefix\" has to be either \"true\" or \"false\"" };

            if (!bool.TryParse(config.GetValue("useOAuth").Value<string>(), out bool useOAuth))
                return new ConfigResult { IsValid = false, Message = "\"useOAuth\" has to be either \"true\" or \"false\"" };


            return new ConfigResult
            {
                IsValid = true,
                ParsedConfig = new Config(config.GetValue("discordToken").Value<string>(), config.GetValue("prefix").Value<string>(), allowMentionPrefix, useOAuth)
            };
        }
    }
}
