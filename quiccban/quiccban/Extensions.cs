using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using quiccban.API.Entities;
using quiccban.Services.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
            "useWebUI"
        };


        public static ConfigResult ParseConfig(this JObject config)
        {

            string clientSecret = null;
            ulong? clientIdNullable = null;

            if (!RequiredConfigValues.All(x => config.Properties().Any(y => y.Name == x)))
                return new ConfigResult { IsValid = false, Message = "Required config values are missing." };

            if (RequiredConfigValues.Any(x => string.IsNullOrWhiteSpace(config.Properties().First(y => y.Name == x).Value.ToString())))
                return new ConfigResult { IsValid = false, Message = "Required config values are null or empty." };

            if (!bool.TryParse(config.GetValue("allowMentionPrefix").Value<string>(), out bool allowMentionPrefix))
                return new ConfigResult { IsValid = false, Message = "\"allowMentionPrefix\" has to be either \"true\" or \"false\"" };

            if (!bool.TryParse(config.GetValue("useWebUI").Value<string>(), out bool useWebUI))
                return new ConfigResult { IsValid = false, Message = "\"useWebUI\" has to be either \"true\" or \"false\"" };

            if (useWebUI)
            {
                if (config.Properties().Any(x => x.Name == "clientId"))
                {
                    if (!ulong.TryParse(config.GetValue("clientId").Value<string>(), out ulong clientId))
                        return new ConfigResult { IsValid = false, Message = "Web UI is turned on, couldn't parse \"clientId\"." };
                    else clientIdNullable = clientId;
                }
                else
                    return new ConfigResult { IsValid = false, Message = "Web UI is turned on, couldn't find \"clientId\"." };


                if (config.Properties().Any(x => x.Name == "clientSecret"))
                {
                    if (string.IsNullOrWhiteSpace(config.GetValue("clientSecret").Value<string>()))
                        return new ConfigResult { IsValid = false, Message = "Web UI is turned on, but \"clientSecret\" is null or empty." };
                    else clientSecret = config.GetValue("clientSecret").Value<string>();
                }
                else
                    return new ConfigResult { IsValid = false, Message = "Web UI is turned on, couldn't find \"clientSecret\"." };
            }


            return new ConfigResult
            {
                IsValid = true,
                ParsedConfig = new Config(config.GetValue("discordToken").Value<string>(), config.GetValue("prefix").Value<string>(), allowMentionPrefix, useWebUI, clientSecret, clientIdNullable)
            };
        }

        public static async Task<SelfUser> ToSelfUserAsync(this IEnumerable<Claim> claims, DiscordService discordService)
        {
            var appInfo = await discordService.discordClient.GetApplicationInfoAsync();
            var idClaimParsed = ulong.Parse(claims.FirstOrDefault(x => x.Type == "id").Value);
            var premiumTypeClaim = claims.FirstOrDefault(x => x.Type == "premiumType");

            return new SelfUser
            {
                AvatarHash = claims.FirstOrDefault(x => x.Type == "avatarHash").Value,
                Id = idClaimParsed,
                Username = claims.FirstOrDefault(x => x.Type == "username").Value,
                Discriminator = ushort.Parse(claims.FirstOrDefault(x => x.Type == "discriminator").Value),
                Flags = ushort.Parse(claims.FirstOrDefault(x => x.Type == "flags").Value),
                IsBotOwner = appInfo.Owner.Id == idClaimParsed,
                PremiumType = premiumTypeClaim == null ? null : new ushort?(ushort.Parse(premiumTypeClaim.Value)),
            };
        }
    }
}
