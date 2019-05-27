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
            {"requires_higher_hierarchy", "You can't do moderator actions on this user." },
            {"automod_update_already_same", "AutoMod is already {0}." },
            {"automod_update_success", "AutoMod has been {0}." },
            {"require_automod_for_modification", "AutoMod has to be enabled to modify this." },
            {"spam_automod_update_already_same", "Spam AutoMod is already {0}." },
            {"spam_automod_update_success", "Spam AutoMod has been {0}." },
            {"require_spam_automod_for_modification", "Spam AutoMod has to be enabled to modify this." },
            {"spam_threshold_already_same", "Spam AutoMod's threshold is already set to {0}." },
            {"spam_threshold_success", "Spam AutoMod's message threshold has been set to {0}." },
            {"spam_action_already_same", "Spam AutoMod's action is already set to {0}." },
            {"spam_action_success", "Spam AutoMod's threshold action has been set to {0} with {1}." },
            {"log_channel_already_same", "Logging channel is already set to {1}." },
            {"log_channel_success", "Logging channel has been set to {1}." },
            {"log_style_already_same", "Logging style is already set to {0}." },
            {"log_style_success", "Logging style has been set to {0}." },
            {"warn_expiry_already_same", "Warn expiry is already set to {0}." },
            {"warn_expiry_success", "Warn expiry has been set to {0}." },
            {"warn_threshold_already_same", "Warn threshold is already set to {0}." },
            {"warn_threshold_success", "Warn threshold has been set to {0}." },
            {"warn_threshold_action_already_same", "Warn threshold action is already set to {0}." },
            {"warn_threshold_action_success", "Warn threshold action has been set to {0}." },
            {"temp_action_require_time", "Temporary actions require a time value." },
            {"value_not_allowed", "That value is not allowed" }
        };



        public string Get(string key)
            => Responses.GetValueOrDefault(key);


    }
}
