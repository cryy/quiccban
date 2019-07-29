using Newtonsoft.Json;
using NJsonSchema.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban
{
    public struct Config
    {
        [Required]
        [JsonProperty("discordToken")]
        public string DiscordToken { get; set; }
        [Required]
        [JsonProperty("prefix")]
        public string Prefix { get; set; }
        [Required]
        [JsonProperty("allowMentionPrefix")]
        public bool AllowMentionPrefix { get; set; }
        [Required]
        [JsonProperty("webUI")]
        public WebUI Web { get; set;  }
    }

    public struct WebUI
    {
        [Required]
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        [JsonProperty("ports")]
        public ushort[] Ports { get; set; }
        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }
        [JsonProperty("clientId")]
        public string ClientId { get; set; }
    }
}
