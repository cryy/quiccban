using quiccban.Database.Models;
using quiccban.Services.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using ActionType = quiccban.Database.Models.ActionType;
using Discord.Rest;
using Discord.WebSocket;
using System.Threading;

namespace quiccban.Services
{
    public class HelperService
    {
        private IServiceProvider _serviceProvider;


        public HelperService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<string> ConstructCaseMessageAsync(Case @case)
        {
            var sb = new StringBuilder();
            var discordService = _serviceProvider.GetService<DiscordService>();
            var databaseService = _serviceProvider.GetService<DatabaseService>();
            var config = _serviceProvider.GetService<Config>();

            IUser target = discordService.discordClient.GetUser(@case.TargetId) as IUser ?? await discordService.discordClient.Rest.GetUserAsync(@case.TargetId);
            IUser mod = discordService.discordClient.GetUser(@case.IssuerId) as IUser ?? await discordService.discordClient.Rest.GetUserAsync(@case.IssuerId);

            switch (@case.Guild.LogStyle)
            {
                case LogStyle.Basic:
                    sb.AppendLine($"**Case #{@case.Id}** | {@case.ActionType}");
                    sb.AppendLine("");
                    sb.AppendLine($"**User:** {target} [{target.Id}] [{target.Mention}]");
                    sb.AppendLine($"**Reason**: {(@case.Reason == null ? $"``Responsible moderator please do {config.Prefix} reason {@case.Id} <reason>``" : @case.Reason)}");
                    sb.AppendLine($"**Responsible moderator**: {mod}");

                    if (@case.ActionType == ActionType.Warn || @case.ActionType == ActionType.Tempban || @case.ActionType == ActionType.Tempmute)
                    {
                        sb.AppendLine($"**Expiration date:** {(@case.ActionType == ActionType.Warn ? DateTimeOffset.UtcNow + TimeSpan.FromSeconds(@case.Guild.WarnExpiry) : @case.GetEndingTime())}");
                        sb.AppendLine($"**Expired**: {(@case.Resolved ? "Yes" : "No")}");
                    }

                    return sb.ToString();
                case LogStyle.Modern:
                    sb.AppendLine($"\u200b  ▫ **User**: {target} [{target.Id}] [{target.Mention}]");
                    sb.AppendLine($"\u200b  ▫ **Reason**: {(@case.Reason == null ? $"``Responsible moderator please do {config.Prefix} reason {@case.Id} <reason>``" : @case.Reason)}");
                    sb.AppendLine($"\u200b  ▫ **Responsible moderator**: {mod}");
                    if (@case.ActionType == ActionType.Warn || @case.ActionType == ActionType.Tempban || @case.ActionType == ActionType.Tempmute)
                    {
                        sb.AppendLine($"\u200b  ▫ **Expiration date:** {(@case.ActionType == ActionType.Warn ? DateTimeOffset.UtcNow + TimeSpan.FromSeconds(@case.Guild.WarnExpiry) : @case.GetEndingTime())}");
                        sb.AppendLine($"\u200b  ▫ **Expired**: {(@case.Resolved ? "Yes" : "No")} {(@case.ForceResolved ? "(Forced)" : "")}");
                    }
                    return sb.ToString();
                default:
                    return "";

            }
        }

        public string ConstructConfigMessage(Guild dbGuild, IGuild guild)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Showing config values for ``{guild.Name}``");
            sb.AppendLine("");
            sb.AppendLine($"\u200b   ⚙**AutoMod**:");
            sb.AppendLine($"\u200b         ⚙**Enabled**: {dbGuild.AutoMod.Enabled}");
            if (dbGuild.AutoMod.Enabled)
            {
                sb.AppendLine($"\u200b         ⚙**Spam AutoMod**: ");
                sb.AppendLine($"\u200b               ⚙**Enabled**: {dbGuild.AutoMod.SpamEnabled}");
                sb.AppendLine($"\u200b               ⚙**Message Threshold**: {dbGuild.AutoMod.SpamMessageThreshold}");
                sb.AppendLine($"\u200b               ⚙**Threshold Action**: {dbGuild.AutoMod.SpamActionType}");
                if (dbGuild.AutoMod.SpamActionExpiry > 0)
                    sb.AppendLine($"\u200b               ⚙**Spam Action Expiry**: {dbGuild.AutoMod.SpamActionExpiry}s");
            }
            sb.AppendLine($"\u200b   ⚙**Log Channel**: {(dbGuild.LogChannelId != 0 ? $"<#{dbGuild.LogChannelId}>" : "Not set")}");
            sb.AppendLine($"\u200b   ⚙**Logging Style**: {dbGuild.LogStyle}");
            sb.AppendLine($"\u200b   ⚙**Warns**:");
            sb.AppendLine($"\u200b         ⚙**Expiry**: {dbGuild.WarnExpiry}s");
            sb.AppendLine($"\u200b         ⚙**Threshold**: {dbGuild.WarnThreshold}");
            sb.AppendLine($"\u200b         ⚙**Threshold Action**: {dbGuild.WarnThresholdActionType}");
            if (dbGuild.WarnThresholdActionExpiry > 0)
                sb.AppendLine($"\u200b         ⚙**Threshold Action Expiry**: {dbGuild.WarnThresholdActionExpiry}");

            return sb.ToString();
        }

        public async Task<RestRole> CreateMuteRoleAsync(Guild dbGuild, bool doLock = true)
        {
            var discordService = _serviceProvider.GetService<DiscordService>();
            var databaseService = _serviceProvider.GetService<DatabaseService>();

            var guild = discordService.discordClient.GetGuild(dbGuild.Id);

            var role = await guild.CreateRoleAsync("Muted", new GuildPermissions(sendMessages: false, speak: false), options: new RequestOptions { AuditLogReason = "Auto role creation." });

            dbGuild.MuteRoleId = role.Id;

            await databaseService.UpdateGuildAsync(dbGuild, doLock);


            foreach (var channel in guild.Channels)
            {
                await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny, speak: PermValue.Deny));
            }

            return role;
        }
    }
}
