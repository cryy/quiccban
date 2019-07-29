using Discord;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Humanizer;
using Qmmands;
using quiccban.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActionType = quiccban.Database.Models.ActionType;

namespace quiccban.Services.Discord.Commands.Modules
{
    [Group("config")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
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

        [Command]
        public Task<CommandResult> GetConfigValuesNameless() => GetConfigValues();

        [Command("get")]
        public async Task<CommandResult> GetConfigValues()
        {
            var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
            return new QuiccbanSuccessResult(_helperService.ConstructConfigMessage(g, Context.Guild));
        }

        [Command("modlogchannel")]
        public async Task<CommandResult> SetModlogChannel(SocketTextChannel value)
        {
            var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
            if (value.Id == g.ModlogChannelId)
                return new QuiccbanFailResult(string.Format(_responseService.Get("modlog_channel_already_same"), value.Id, value.Mention));

            g.ModlogChannelId = value.Id;

            await _databaseService.UpdateGuildAsync(g);
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("modlog_channel_success"), value.Id, value.Mention));
        }

        [Command("logstyle")]
        public async Task<CommandResult> SetLogStyle(LogStyle value)
        {
            var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
            if (value == g.LogStyle)
                return new QuiccbanFailResult(string.Format(_responseService.Get("log_style_already_same"), value));

            g.LogStyle = value;

            await _databaseService.UpdateGuildAsync(g);
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("log_style_success"), value));
        }

        [Command("warnexpiry")]
        public async Task<CommandResult> SetWarnExpiry(TimeSpan value)
        {
            var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
            if ((int)value.TotalSeconds == g.WarnExpiry)
                return new QuiccbanFailResult(string.Format(_responseService.Get("warn_expiry_already_same"), value.Humanize()));

            g.WarnExpiry = (int)value.TotalSeconds;

            await _databaseService.UpdateGuildAsync(g);
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("warn_expiry_success"), value.Humanize()));
        }

        [Command("warnthreshold")]
        public async Task<CommandResult> SetThreshold(int value)
        {
            var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
            if (value == g.WarnThreshold)
                return new QuiccbanFailResult(string.Format(_responseService.Get("warn_threshold_already_same"), value));

            g.WarnThreshold = value;

            await _databaseService.UpdateGuildAsync(g);
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("warn_threshold_success"), value));
        }

        [Command("warnthresholdaction")]
        public async Task<CommandResult> SetThresholdAction([IsntActionType(ActionType.Unban, ActionType.Unmute, ActionType.Unwarn, ActionType.Warn, ActionType.HackBan)]
            ActionType value, TimeSpan? expiry = null)
        {
            var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

            if (value == g.WarnThresholdActionType && ((expiry != null && (int)expiry.Value.TotalSeconds == g.WarnThresholdActionExpiry) || (expiry == null && g.WarnThresholdActionExpiry == 0)))
                return new QuiccbanFailResult(string.Format(_responseService.Get("warn_threshold_already_same"), value));

            if ((value == ActionType.TempBan || value == ActionType.TempMute) && expiry == null)
                return new QuiccbanFailResult(string.Format(_responseService.Get("temp_action_require_time")));

            g.WarnThresholdActionType = value;
            g.WarnThresholdActionExpiry = expiry != null ? (int)expiry?.TotalSeconds : 0;

            await _databaseService.UpdateGuildAsync(g);
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("warn_threshold_action_success"), value, expiry == null ? "no expiry" : expiry.Value.Humanize(4) + " expiry"));
        }

        [Group("automod")]
        public class ConfigurationModule_Automod : QuiccbanModule<QuiccbanContext>
        {
            DatabaseService _databaseService;
            ResponseService _responseService;
            public ConfigurationModule_Automod(DatabaseService databaseService, ResponseService responseService)
            {
                _databaseService = databaseService;
                _responseService = responseService;
            }

            [Command]
            public async Task<CommandResult> SetAutomod(bool value)
            {
                var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
                if (value == g.AutoMod.Enabled)
                    return new QuiccbanFailResult(string.Format(_responseService.Get("automod_update_already_same"), value ? "Enabled" : "Disabled"));

                g.AutoMod.Enabled = value;

                await _databaseService.UpdateGuildAsync(g);
                return new QuiccbanSuccessResult(string.Format(_responseService.Get("automod_update_success"), value ? "Enabled" : "Disabled"));
            }

            [Command("spam")]
            [RequireAutoMod]
            public async Task<CommandResult> SetAutomodSpam(bool value)
            {
                var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

                if (value == g.AutoMod.SpamEnabled)
                    return new QuiccbanFailResult(string.Format(_responseService.Get("spam_automod_update_already_same"), value ? "Enabled" : "Disabled"));

                g.AutoMod.SpamEnabled = value;

                await _databaseService.UpdateGuildAsync(g);
                return new QuiccbanSuccessResult(string.Format(_responseService.Get("spam_automod_update_success"), value ? "Enabled" : "Disabled"));
            }

            [Command("raid")]
            [RequireAutoMod]
            public async Task<CommandResult> SetAutomodRaid(bool value)
            {
                var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

                if (value == g.AutoMod.RaidEnabled)
                    return new QuiccbanFailResult(string.Format(_responseService.Get("raid_automod_update_already_same"), value ? "Enabled" : "Disabled"));

                g.AutoMod.RaidEnabled = value;

                await _databaseService.UpdateGuildAsync(g);
                return new QuiccbanSuccessResult(string.Format(_responseService.Get("raid_automod_update_success"), value ? "Enabled" : "Disabled"));
            }
        }
    }
}
