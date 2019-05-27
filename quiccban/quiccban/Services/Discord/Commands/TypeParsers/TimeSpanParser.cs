using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace quiccban.Services.Discord.Commands
{
    public sealed class TimeSpanParser : TypeParser<TimeSpan>
    {
        private static Regex _pattern = new Regex(@"(\d+[dhms])", RegexOptions.Compiled);
        public override Task<TypeParserResult<TimeSpan>> ParseAsync(Parameter parameter, string value, ICommandContext ctx, IServiceProvider provider)
        {
            var result = TimeSpan.Zero;
            var input = value.ToLower();
            var matches = _pattern.Matches(input)
                .Select(m => m.Value);

            foreach (var match in matches)
            {
                var amount = double.Parse(match.Substring(0, match.Length - 1));
                switch (match[match.Length - 1])
                {
                    case 'd': result = result.Add(TimeSpan.FromDays(amount)); break;
                    case 'h': result = result.Add(TimeSpan.FromHours(amount)); break;
                    case 'm': result = result.Add(TimeSpan.FromMinutes(amount)); break;
                    case 's': result = result.Add(TimeSpan.FromSeconds(amount)); break;
                }
            }

            if (result == TimeSpan.Zero)
                return Task.FromResult(new TypeParserResult<TimeSpan>("Failed to parse TimeSpan"));
            return Task.FromResult(new TypeParserResult<TimeSpan>(result));
        }
    }
}
