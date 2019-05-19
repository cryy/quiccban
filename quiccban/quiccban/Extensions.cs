using Microsoft.Extensions.Configuration;
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
            "allowMentionPrefix"
        };


        public static ConfigResult ParseConfig(this IConfiguration config)
        {
           
            var configEnumerable = config.AsEnumerable();

            if (!RequiredConfigValues.All(x => configEnumerable.Any(y => y.Key == x)))
                return new ConfigResult { IsValid = false, Message = "Required config values are missing." };

            if (RequiredConfigValues.Any(x => string.IsNullOrWhiteSpace(configEnumerable.First(y => y.Key == x).Value)))
                return new ConfigResult { IsValid = false, Message = "Required config values are null or empty." };

            if(!bool.TryParse(config.GetValue<string>("allowMentionPrefix"), out bool allowMentionPrefix))
                return new ConfigResult { IsValid = false, Message = "\"allowMentionPrefix\" has to be either \"true\" or \"false\"" };


            return new ConfigResult
            {
                IsValid = true,
                ParsedConfig = new Config(config.GetValue<string>("discordToken"), config.GetValue<string>("prefix"), allowMentionPrefix)
            };
        }
    }
}
