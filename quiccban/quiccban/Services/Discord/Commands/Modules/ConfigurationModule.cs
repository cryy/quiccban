using Discord;
using Discord.WebSocket;
using Humanizer;
using Qmmands;
using quiccban.Database.Models;
using quiccban.Services.Discord.Commands.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActionType = quiccban.Database.Models.ActionType;

namespace quiccban.Services.Discord.Commands.Modules
{
    [Group("config")]
    public class ConfigurationModule : QuiccbanModule<QuiccbanContext>
    {
        DatabaseService _databaseService;
        HelperService _helperService;
        ResponseService _responseService;

        public ConfigurationModule(DatabaseService databaseService, HelperService helperService, ResponseService responseService)
        {
            _databaseService = databaseService;
            _helperService = helperService;
            _responseService = responseService;
        }

        [Command("get")]
        public async Task GetConfigValues()
        {
            var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
            await ReplyAsync(_helperService.ConstructConfigMessage(g, Context.Guild));
        }

        [Group("set")]
        public class ConfigurationModule_Set : QuiccbanModule<QuiccbanContext>
        {
            DatabaseService _databaseService;
            ResponseService _responseService;

            public ConfigurationModule_Set(DatabaseService databaseService, ResponseService responseService)
            {
                _databaseService = databaseService;
                _responseService = responseService;
            }

            [Group("automod")]
            public class ConfigurationModule_Set_Automod : QuiccbanModule<QuiccbanContext>
            {
                DatabaseService _databaseService;
                ResponseService _responseService;
                public ConfigurationModule_Set_Automod(DatabaseService databaseService, ResponseService responseService)
                {
                    _databaseService = databaseService;
                    _responseService = responseService;
                }

                [Command]
                public async Task SetAutomod(bool value)
                {
                    var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
                    if(value == g.AutoMod.Enabled)
                    {
                        await ReplyAsync(string.Format(_responseService.Get("automod_update_already_same"), value ? "Enabled" : "Disabled"));
                        return;
                    }

                    g.AutoMod.Enabled = value;

                    await _databaseService.UpdateGuildAsync(g);
                    await ReplyAsync(string.Format(_responseService.Get("automod_update_success"), value ? "Enabled" : "Disabled"));
                }

                [Command("spam")]
                [RequireAutoMod]
                public async Task SetAutomodSpam(bool value)
                {
                    var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

                    if (value == g.AutoMod.SpamEnabled)
                    {
                        await ReplyAsync(string.Format(_responseService.Get("spam_automod_update_already_same"), value ? "Enabled" : "Disabled"));
                        return;
                    }

                    g.AutoMod.SpamEnabled = value;

                    await _databaseService.UpdateGuildAsync(g);
                    await ReplyAsync(string.Format(_responseService.Get("spam_automod_update_success"), value ? "Enabled" : "Disabled"));
                }

                [Command("spamthreshold")]
                [RequireAutoMod]
                [RequireAutoModSpam]
                public async Task SetAutomodSpamThreshold(int value)
                {
                    var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

                    if (value == g.AutoMod.SpamMessageThreshold)
                    {
                        await ReplyAsync(string.Format(_responseService.Get("spam_threshold_already_same"), value));
                        return;
                    }

                    g.AutoMod.SpamMessageThreshold = value;

                    await _databaseService.UpdateGuildAsync(g);
                    await ReplyAsync(string.Format(_responseService.Get("spam_threshold_success"), value));
                }

                [Command("spamaction")]
                [RequireAutoMod]
                [RequireAutoModSpam]
                public async Task SetAutomodSpamAction([IsntActionType(ActionType.Unban, ActionType.Unmute, ActionType.Unwarn)]
                ActionType value, TimeSpan? expiry = null)
                {
                    var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

                    if (value == g.AutoMod.SpamActionType && ((expiry != null && (int)expiry.Value.TotalSeconds == g.AutoMod.SpamActionExpiry) || (expiry == null && g.AutoMod.SpamActionExpiry == 0)))
                    {
                        await ReplyAsync(string.Format(_responseService.Get("spam_action_already_same"), value));
                        return;
                    }

                    if((value == ActionType.Tempban || value == ActionType.Tempmute) && expiry == null)
                    {
                        await ReplyAsync(string.Format(_responseService.Get("temp_action_require_time")));
                        return;
                    }

                    g.AutoMod.SpamActionType = value;
                    if (expiry != null)
                        g.AutoMod.SpamActionExpiry = (int)expiry?.TotalSeconds;

                    await _databaseService.UpdateGuildAsync(g);
                    await ReplyAsync(string.Format(_responseService.Get("spam_threshold_success"), value, expiry == null ? "no expiry" : expiry.Value.Humanize(4) + " expiry"));
                }


            }

            [Command("logchannel")]
            public async Task SetLogChannel(SocketTextChannel value)
            {
                var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
                if (value.Id == g.LogChannelId)
                {
                    await ReplyAsync(string.Format(_responseService.Get("log_channel_already_same"), value.Id, value.Mention));
                    return;
                }

                g.LogChannelId = value.Id;

                await _databaseService.UpdateGuildAsync(g);
                await ReplyAsync(string.Format(_responseService.Get("log_channel_success"), value.Id, value.Mention));
            }

            [Command("logstyle")]
            public async Task SetLogStyle(LogStyle value)
            {
                var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
                if (value == g.LogStyle)
                {
                    await ReplyAsync(string.Format(_responseService.Get("log_style_already_same"), value));
                    return;
                }

                g.LogStyle = value;

                await _databaseService.UpdateGuildAsync(g);
                await ReplyAsync(string.Format(_responseService.Get("log_style_success"), value));
            }

            [Command("warnexpiry")]
            public async Task SetWarnExpiry(TimeSpan value)
            {
                var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
                if ((int)value.TotalSeconds == g.WarnExpiry)
                {
                    await ReplyAsync(string.Format(_responseService.Get("warn_expiry_already_same"), value.Humanize()));
                    return;
                }

                g.WarnExpiry = (int)value.TotalSeconds;

                await _databaseService.UpdateGuildAsync(g);
                await ReplyAsync(string.Format(_responseService.Get("warn_expiry_success"), value.Humanize()));
            }

            [Command("warnthreshold")]
            public async Task SetThreshold(int value)
            {
                var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
                if (value == g.WarnThreshold)
                {
                    await ReplyAsync(string.Format(_responseService.Get("warn_threshold_already_same"), value));
                    return;
                }

                g.WarnThreshold = value;

                await _databaseService.UpdateGuildAsync(g);
                await ReplyAsync(string.Format(_responseService.Get("warn_threshold_success"), value));
            }

            [Command("warnthresholdaction")]
            public async Task SetThresholdAction([IsntActionType(ActionType.Unban, ActionType.Unmute, ActionType.Unwarn, ActionType.Warn)]
            ActionType value, TimeSpan? expiry = null)
            {
                var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

                if (value == g.WarnThresholdActionType && ((expiry != null && (int)expiry.Value.TotalSeconds == g.WarnThresholdActionExpiry) || (expiry == null && g.WarnThresholdActionExpiry == 0)))
                {
                    await ReplyAsync(string.Format(_responseService.Get("warn_threshold_already_same"), value));
                    return;
                }

                if ((value == Database.Models.ActionType.Tempban || value == Database.Models.ActionType.Tempmute) && expiry == null)
                {
                    await ReplyAsync(string.Format(_responseService.Get("temp_action_require_time")));
                    return;
                }

                g.WarnThresholdActionType = value;
                if (expiry != null)
                    g.WarnThresholdActionExpiry = (int)expiry?.TotalSeconds;

                await _databaseService.UpdateGuildAsync(g);
                await ReplyAsync(string.Format(_responseService.Get("warn_threshold_action_success"), value));
            }
        }
    }
}
