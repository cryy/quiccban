using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.Services
{
    public class ResponseService
    {
        private Dictionary<string, string> Responses = new Dictionary<string, string>()
        {
            {"isnt_case_owner", "That case isn't yours."},
            {"reason_success", "👌" },
            {"warn_success", "Warned {0}." },
            {"tempmute_success", "Temporarily muted {0} for {2}." },
            {"mute_success", "Muted {0}." },
            {"user_already_muted", "{0} is already muted." },
            {"requires_higher_equal_hierarchy", "You can't do moderator actions on this user." },
            {"requires_higher_hierarchy", "You can't do moderator actions on this user." }
        };



        public string Get(string key)
            => Responses.GetValueOrDefault(key);


    }
}
