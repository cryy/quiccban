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
using Humanizer;
using quiccban.Services.Discord.Commands.Objects;

namespace quiccban.Services
{
    public class HelperService
    {
        private IServiceProvider _serviceProvider;

        public HelperService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<string> ConstructCaseMessageAsync(Case @case, bool external = false)
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
                    sb.AppendLine($"**Case #{@case.Id}** | {@case.ActionType.Humanize()}");
                    sb.AppendLine("");
                    sb.AppendLine($"**User:** {target} [{target.Id}] {(external ? "" : $"[{target.Mention}]")}");
                    sb.AppendLine($"**Reason**: {(@case.Reason == null ? $"``Responsible moderator please do {config.Prefix} reason {@case.Id} <reason>``" : string.Format(@case.Reason, @case.Id))}");
                    sb.AppendLine($"**Responsible moderator**: {mod}");
                    if (@case.ActionExpiry > 0 || @case.ActionType == ActionType.Warn)
                    {
                        sb.AppendLine($"**Expiration date:** {(external ? (@case.ActionType == ActionType.Warn ? DateTimeOffset.UtcNow + TimeSpan.FromSeconds(@case.Guild.WarnExpiry) : @case.GetEndingTime()).Humanize() : (@case.ActionType == ActionType.Warn ? DateTimeOffset.UtcNow + TimeSpan.FromSeconds(@case.Guild.WarnExpiry) : @case.GetEndingTime()).ToString("dd/MM/yyyy H:mm:ss zzz") + " (dd/MM)")}");
                        sb.AppendLine($"**Expired**: {(@case.Resolved ? "Yes" : "No")}");
                    }

                    return sb.ToString();
                case LogStyle.Modern:
                    sb.AppendLine($"\u200b\u3000▫ **User**: {target} [{target.Id}] [{target.Mention}]");
                    sb.AppendLine($"\u200b\u3000▫ **Reason**: {(@case.Reason == null ? $"``Responsible moderator please do {config.Prefix} reason {@case.Id} <reason>``" : string.Format(@case.Reason, @case.Id))}");
                    sb.AppendLine($"\u200b\u3000▫ **Responsible moderator**: {mod}");
                    if (@case.ActionExpiry > 0 || @case.ActionType == ActionType.Warn)
                    {
                        sb.AppendLine($"\u200b\u3000▫ **Expiration date:** {(external ? (@case.ActionType == ActionType.Warn ? DateTimeOffset.UtcNow + TimeSpan.FromSeconds(@case.Guild.WarnExpiry) : @case.GetEndingTime()).Humanize() : (@case.ActionType == ActionType.Warn ? DateTimeOffset.UtcNow + TimeSpan.FromSeconds(@case.Guild.WarnExpiry) : @case.GetEndingTime()).ToString("dd/MM/yyyy H:mm:ss zzz") + " (dd/MM)")}");
                        sb.AppendLine($"\u200b\u3000▫ **Expired**: {(@case.Resolved ? "Yes" : "No")} {(@case.ForceResolved ? "(Forced)" : "")}");
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
            sb.AppendLine($"\u200b\u2007⚙**AutoMod**:");
            sb.AppendLine($"\u2007\u3000⚙**Enabled**: {dbGuild.AutoMod.Enabled}");
            if (dbGuild.AutoMod.Enabled)
            {
                sb.AppendLine($"\u2007\u3000⚙**Spam AutoMod**: ");
                sb.AppendLine($"\u2007\u3000⚙**Raid AutoMod**: ");
            }
            sb.AppendLine($"\u2007⚙**Modlog Channel**: {(dbGuild.ModlogChannelId != 0 ? $"<#{dbGuild.ModlogChannelId}>" : "Not set")}");
            sb.AppendLine($"\u2007⚙**Logging Style**: {dbGuild.LogStyle}");
            sb.AppendLine($"\u2007⚙**Warns**:");
            sb.AppendLine($"\u2007\u3000⚙**Expiry**: {TimeSpan.FromSeconds(dbGuild.WarnExpiry).Humanize()}");
            sb.AppendLine($"\u2007\u3000⚙**Threshold**: {dbGuild.WarnThreshold}");
            sb.AppendLine($"\u2007\u3000⚙**Threshold Action**: {dbGuild.WarnThresholdActionType.Humanize()}");
            if (dbGuild.WarnThresholdActionExpiry > 0)
                sb.AppendLine($"\u2007\u3000\u3000⚙**Threshold Action Expiry**: {TimeSpan.FromSeconds(dbGuild.WarnThresholdActionExpiry).Humanize()}");

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

        public async Task<Embed> ConstructHistoryEmbedAsync(IEnumerable<Case> cases, IUser caseCarrier)
        {
            if (cases.Count() > 5)
                throw new InvalidOperationException("Cannot construct embed with more than 5 cases.");

            var discordService = _serviceProvider.GetService<DiscordService>();

            var eb = new EmbedBuilder();

            eb.WithAuthor($"Case history for {caseCarrier}", caseCarrier.GetAvatarUrl() ?? caseCarrier.GetDefaultAvatarUrl());

            var sb = new StringBuilder();

            foreach (var @case in cases)
            {

                IUser mod = discordService.discordClient.GetUser(@case.IssuerId) as IUser ?? await discordService.discordClient.Rest.GetUserAsync(@case.IssuerId);

                sb.AppendLine(@case.GetDiscordMessageLink() != null ? $"**[Case {@case.Id}]({@case.GetDiscordMessageLink()})**:" : $"**Case {@case.Id}**:");
                sb.AppendLine($"\u3000Type: {@case.ActionType.Humanize()}");
                sb.AppendLine($"\u3000Reason: {(@case.Reason == null ? "No reason has been set." : (@case.Reason.StartsWith("``Responsible moderator please do") ? "No reason has been set" : @case.Reason))}");
                sb.AppendLine($"\u3000Responsible moderator: {mod}");
                if (@case.ActionExpiry > 0 || @case.ActionType == ActionType.Warn)
                {
                    sb.AppendLine($"\u3000Expired: {(@case.Resolved ? "Yes" : "No")} {(@case.ForceResolved ? "(Forced)" : "")}");
                }

            }

            eb.WithDescription(sb.ToString());

            return eb.Build();

        }

        public void FilterCleaningCollection(ref IEnumerable<IUserMessage> c, CleanType[] cleanTypes)
        {
            foreach (var cleanType in cleanTypes)
            {
                switch (cleanType)
                {
                    case CleanType.Attachments:
                        c = c.Where(x => x.Attachments.Count > 0);
                        break;
                    case CleanType.Bots:
                        c = c.Where(x => x.Author.IsBot);
                        break;
                    case CleanType.Users:
                        c = c.Where(x => !x.Author.IsBot);
                        break;
                    case CleanType.Embeds:
                        c = c.Where(x => x.Embeds.Count > 0);
                        break;
                    case CleanType.Mentions:
                        c = c.Where(x => x.MentionedUserIds.Count > 0);
                        break;
                }
            }

        }
    }
}
