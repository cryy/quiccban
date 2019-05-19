using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.Assets
{
    public class JsonBuilder
    {
        public static JObject DefaultJsonConfig() => new JsonBuilder().AddField("discordToken", "")
                                                                      .AddField("prefix", "")
                                                                      .AddField("allowMentionPrefix", true)
                                                                      .GetPrototype();


        private JObject Json;

        public JsonBuilder()
        {
            Json = new JObject();
        }

        public JsonBuilder AddField(string name, object value)
        {
            Json.Add(new JProperty(name, value));
            return this;
        }

        public JObject GetPrototype() =>
            Json;

        public string Raw => ToString();

        public override string ToString() => Json.ToString();
    }
}
