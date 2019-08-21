using Humanizer;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Generation;
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
            var validator = JsonSchema.FromType<Config>(new JsonSchemaGeneratorSettings { AlwaysAllowAdditionalObjectProperties = true});
            

            var validationResult = validator.Validate(config);

            if (validationResult.Count > 0)
                return new ConfigResult { IsValid = false, Message = "\n\t\t\t\t" + string.Join("\n\t\t\t\t", validationResult.Select(x => $"Line {x.LineNumber} | {x.Kind.Humanize()} (p: {x.Path})")) };

            var configObject = JsonConvert.DeserializeObject<Config>(config.ToString());

            if (configObject.Web.Enabled)
            {
                if (configObject.Web.Ports is null || configObject.Web.Ports.Length == 0)
                    return new ConfigResult { IsValid = false, Message = "Web UI ports aren't defined." };

                if (string.IsNullOrWhiteSpace(configObject.Web.ClientId))
                    return new ConfigResult { IsValid = false, Message = "Web UI client id isn't defined." };

                if (string.IsNullOrWhiteSpace(configObject.Web.ClientSecret))
                    return new ConfigResult { IsValid = false, Message = "Web UI client secret isn't defined." };
            }

            return new ConfigResult { IsValid = true, ParsedConfig = configObject };
        }

      
    }
}
