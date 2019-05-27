using Discord;
using Discord.WebSocket;
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

        public ConfigurationModule(DatabaseService databaseService, HelperService helperService)
        {
            _databaseService = databaseService;
            _helperService = helperService;
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

            public ConfigurationModule_Set(DatabaseService databaseService)
            {
                _databaseService = databaseService;
            }

            [Group("automod")]
            public class ConfigurationModule_Set_Automod : QuiccbanModule<QuiccbanContext>
            {
                DatabaseService _databaseService;

                public ConfigurationModule_Set_Automod(DatabaseService databaseService)
                {
                    _databaseService = databaseService;
                }

                [Command]
                public async Task SetAutomod(bool value)
                {
                    var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
                    if(value == g.AutoMod.Enabled)
                    {
                        await ReplyAsync($"❌ | AutoMod is already {(value ? "enabled" : "disabled")}.");
                        return;
                    }

                    g.AutoMod.Enabled = value;

                    await _databaseService.UpdateGuildAsync(g);
                    await ReplyAsync($"✅ | {(value ? "Enabled AutoMod." : "Disabled AutoMod.")}");
                }

                [Command("spam")]
                [RequireAutoMod("❌ | AutoMod has to be enabled to modify this.")]
                public async Task SetAutomodSpam(bool value)
                {
                    var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

                    if (value == g.AutoMod.SpamEnabled)
                    {
                        await ReplyAsync($"❌ | Spam AutoMod is already {(value ? "enabled" : "disabled")}.");
                        return;
                    }

                    g.AutoMod.SpamEnabled = value;

                    await _databaseService.UpdateGuildAsync(g);
                    await ReplyAsync($"✅ | {(value ? "Enabled Spam AutoMod." : "Disabled Spam AutoMod.")}");
                }

                [Command("spamthreshold")]
                [RequireAutoMod("❌ | AutoMod has to be enabled to modify this.")]
                [RequireAutoModSpam("❌ | Spam AutoMod has to be enabled to modify this.")]
                public async Task SetAutomodSpamThreshold(int value)
                {
                    var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

                    if (value == g.AutoMod.SpamMessageThreshold)
                    {
                        await ReplyAsync($"❌ | Spam AutoMod's threshold is already set to {value}.");
                        return;
                    }

                    g.AutoMod.SpamMessageThreshold = value;

                    await _databaseService.UpdateGuildAsync(g);
                    await ReplyAsync($"✅ | Spam AutoMod's message threshold has been set to {value}.");
                }

                [Command("spamaction")]
                [RequireAutoMod("❌ | AutoMod has to be enabled to modify this.")]
                [RequireAutoModSpam("❌ | Spam AutoMod has to be enabled to modify this.")]
                public async Task SetAutomodSpamAction(ActionType value, TimeSpan? expiry = null)
                {
                    var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

                    if (value == ActionType.Unban || value == ActionType.Unmute || value == ActionType.Unmute)
                    {
                        await ReplyAsync($"❌ | Invalid action type.");
                        return;
                    }

                    if (value == g.AutoMod.SpamActionType)
                    {
                        await ReplyAsync($"❌ | Spam AutoMod's threshold action is already {value}.");
                        return;
                    }

                    if((value == ActionType.Tempban || value == ActionType.Tempmute) && expiry == null)
                    {
                        await ReplyAsync($"❌ | Temporary actions require a time value.");
                        return;
                    }

                    g.AutoMod.SpamActionType = value;
                    if (expiry != null)
                        g.AutoMod.SpamActionExpiry = (int)expiry?.TotalSeconds;

                    await _databaseService.UpdateGuildAsync(g);
                    await ReplyAsync($"✅ | Spam AutoMod's threshold action has been set to {value} {(expiry != null ? $"with {(int)expiry?.TotalSeconds}s expiry" : "")}.");
                }


            }

            [Command("logchannel")]
            public async Task SetLogChannel(SocketTextChannel value)
            {
                var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
                if (value.Id == g.LogChannelId)
                {
                    await ReplyAsync($"❌ | Log channel is already set to {value.Mention}.");
                    return;
                }

                g.LogChannelId = value.Id;

                await _databaseService.UpdateGuildAsync(g);
                await ReplyAsync($"✅ | Set logging channel to {value.Mention}.");
            }

            [Command("logstyle")]
            public async Task SetLogStyle(LogStyle value)
            {
                var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
                if (value == g.LogStyle)
                {
                    await ReplyAsync($"❌ | Logging style is already set to {value}.");
                    return;
                }

                g.LogStyle = value;

                await _databaseService.UpdateGuildAsync(g);
                await ReplyAsync($"✅ | Set logging style to {value}.");
            }

            [Command("warnexpiry")]
            public async Task SetWarnExpiry(TimeSpan value)
            {
                var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
                if ((int)value.TotalSeconds == g.WarnExpiry)
                {
                    await ReplyAsync($"❌ | Warn expiry is already set to {(int)value.TotalSeconds}s.");
                    return;
                }

                g.WarnExpiry = (int)value.TotalSeconds;

                await _databaseService.UpdateGuildAsync(g);
                await ReplyAsync($"✅ | Set warn expiry to {(int)value.TotalSeconds}s.");
            }

            [Command("warnthreshold")]
            public async Task SetThreshold(int value)
            {
                var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
                if (value == g.WarnThreshold)
                {
                    await ReplyAsync($"❌ | Warn threshold is already set to {value}.");
                    return;
                }

                g.WarnThreshold = value;

                await _databaseService.UpdateGuildAsync(g);
                await ReplyAsync($"✅ | Set warn threshold to {value}.");
            }

            [Command("warnthresholdaction")]
            public async Task SetThresholdAction(ActionType value, TimeSpan? expiry = null)
            {
                var g = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

                if(value == ActionType.Unban || value == ActionType.Unmute || value == ActionType.Unmute || value == ActionType.Warn)
                {
                    await ReplyAsync($"❌ | Invalid action type.");
                    return;
                }

                if (value == g.WarnThresholdActionType)
                {
                    await ReplyAsync($"❌ | Warn threshold action is already {value}.");
                    return;
                }

                if ((value == Database.Models.ActionType.Tempban || value == Database.Models.ActionType.Tempmute) && expiry == null)
                {
                    await ReplyAsync($"❌ | Temporary actions require a time value.");
                    return;
                }

                g.WarnThresholdActionType = value;
                if (expiry != null)
                    g.WarnThresholdActionExpiry = (int)expiry?.TotalSeconds;

                await _databaseService.UpdateGuildAsync(g);
                await ReplyAsync($"✅ | Warn threshold action has been set to {value} {(expiry != null ? $"with {(int)expiry?.TotalSeconds}s expiry" : "")}.");
            }
        }
    }
}
