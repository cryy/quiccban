using System.Collections.Generic;

namespace Discord.Addons.Interactive
{
    public class PaginatedMessage
    {
        public IEnumerable<Embed> Pages { get; set; }

        public string Content { get; set; } = "";

        public PaginatedAppearanceOptions Options { get; set; } = PaginatedAppearanceOptions.Default;
    }
}
