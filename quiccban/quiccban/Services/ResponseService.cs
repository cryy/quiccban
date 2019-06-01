using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.Services
{
    public class ResponseService
    {
        private object _overwriteLock = new object();
        private Dictionary<string, string> Responses = new Dictionary<string, string>()
        {
            {"history_no_cases", "{0} has no {2}cases." },
            {"isnt_case_owner", "That case isn't yours."},
            {"reason_success", "👌" },
            {"warn_success", "Warned {0}." },
            {"unwarn_no_warns", "{0} has no active warns." },
            {"unwarn_success", "Unwarned {0}." },
            {"tempmute_success", "Temporarily muted {0} for {2}." },
            {"mute_success", "Muted {0}." },
            {"user_already_muted", "{0} is already muted." },
            {"unmute_not_muted", "{0} isn't muted." },
            {"unmute_success", "Unmuted {0}." },
            {"kick_success", "Kicked {0}." },
            {"tempban_success", "Temporarily banned {0} for {2}." },
            {"ban_success", "Banned {0}." },
            {"hackban_user_in_guild", "That user is already in this guild. Please use the normal ban command." },
            {"hackban_user_not_found", "A user with that id does not exist." },
            {"hackban_fail", "Failed to do a hackban." },
            {"hackban_success", "Successfully hackbanned {0}." },
            {"unban_success", "Unbanned {0}." },
            {"clean_amount_too_large", "Can't clean more than 300 messages at a time." },
            {"clean_no_messages", "Couldn't find any messages." },
            {"clean_success", "Cleaned {0} messages." },
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
            {"warn_threshold_action_success", "Warn threshold action has been set to {0} with {1}." },
            {"temp_action_require_time", "Temporary actions require a time value." },
            {"value_not_allowed", "That value is not allowed" },
            {"require_guild_permission", "You require guild permission {0} to do this." },
            {"require_channel_permission", "You require channel permission {0} to do this." },
            {"bot_require_guild_permission", "I require guild permission {0} to do this." },
            {"bot_require_channel_permission", "I require channel permission {0} to do this." },
            {"require_log_channel", "A logging channel must be defined to use this. Ask an admin to set a logging channel via ``{0}config set logchannel <channel>``" },
            {"log_channel_doesnt_exist", "Couldn't find the log channel. Log channel must be set to do moderation actions." },
            {"require_log_channel_permission", "I require {2} permission in {1}." },
            {"require_higher_equal_hierarchy", "You can't do moderator actions on that user." },
            {"require_higher_hierarchy", "You can't do moderator actions on that user." },
            {"bot_require_higher_equal_hierarchy", "I can't do moderator actions on that user." },
            {"bot_require_higher_hierarchy", "I can't do moderator actions on that user." },
            {"owner_only_command", "This command can only be ran by my owner." }
        };



        public string Get(string key)
            => Responses.GetValueOrDefault(key);

        public void Overwrite(string key, string value)
        {
            lock(_overwriteLock)
            {
                Responses[key] = value;
            }
        }

    }
}
