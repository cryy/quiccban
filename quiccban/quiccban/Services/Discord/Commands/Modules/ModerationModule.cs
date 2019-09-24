using Discord;
using Discord.WebSocket;
using Qmmands;
using quiccban.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActionType = quiccban.Database.Models.ActionType;
using Humanizer;
using Discord.Rest;
using Discord.Net;
using quiccban.Services.Discord.Commands.Objects;

namespace quiccban.Services.Discord.Commands.Modules
{
    [RequireModlogChannel]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public class ModerationModule : QuiccbanModule<QuiccbanContext>
    {
        DatabaseService _databaseService;
        HelperService _helperService;
        CaseHandlingService _caseHandlingService;
        ResponseService _responseService;
        public ModerationModule(DatabaseService databaseService, HelperService helperService, CaseHandlingService caseHandlingService, ResponseService responseService)
        {
            _databaseService = databaseService;
            _helperService = helperService;
            _caseHandlingService = caseHandlingService;
            _responseService = responseService;
        }

        [Command("reason")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task<CommandResult> SetReasonAsync(Case @case, [Remainder]string reason)
        {
            if (@case.IssuerId != Context.User.Id)
                return new QuiccbanFailResult(_responseService.Get("isnt_case_owner"));

            @case.Reason = reason;

            try
            {
                await _databaseService.UpdateCaseAsync(@case, true);
                await _caseHandlingService.UpdateDiscordMessage(@case);
            }
            catch (InvalidOperationException ex)
            {
                return new QuiccbanFailResult(ex.Message);
            }

            return new QuiccbanSuccessResult(string.Format(_responseService.Get("reason_success"), @case.Id));
        }

        [Command("warn")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task<CommandResult> WarnAsync([RequireHigherOrEqualHierarchy, RequireHigherOrEqualBotHierarchy]SocketGuildUser u, [Remainder]string reason = null)
        {
            try
            {
                await _databaseService.CreateNewCaseAsync(Context.Guild, reason, ActionType.Warn, 0, Context.User, u);
            }
            catch (InvalidOperationException ex)
            {
                return new QuiccbanFailResult(ex.Message);
            }
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("warn_success"), u.ToString(), u.Mention));
        }

        [Command("unwarn")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task<CommandResult> UnwarnAsync([RequireHigherOrEqualHierarchy, RequireHigherOrEqualBotHierarchy]SocketGuildUser u, [Remainder]string reason = null)
        {
            var dbGuild = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

            var @case = dbGuild.Cases.LastOrDefault(x => !x.Resolved && x.ActionType == ActionType.Warn && x.TargetId == u.Id);

            if (@case == null)
                return new QuiccbanFailResult(string.Format(_responseService.Get("unwarn_no_warns"), u.ToString(), u.Mention));
            try
            {
                await _databaseService.ResolveCaseAsync(@case, Context.User, reason);
            }
            catch (InvalidOperationException ex)
            {
                return new QuiccbanFailResult(ex.Message);
            }

            return new QuiccbanSuccessResult(string.Format(_responseService.Get("unwarn_success"), u.ToString(), u.Mention));
        }

        [Command("tempmute")]
        [RequireUserPermission(GuildPermission.DeafenMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task<CommandResult> TempMuteAsync([RequireHigherOrEqualHierarchy, RequireHigherOrEqualBotHierarchy]SocketGuildUser u, TimeSpan time, [Remainder]string reason = null)
        {
            var dbGuild = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

            try
            {
                IRole muteRole = Context.Guild.GetRole(dbGuild.MuteRoleId);
                if (muteRole == null)
                    muteRole = await _helperService.CreateMuteRoleAsync(dbGuild);

                if (u.Roles.Any(x => x.Id == muteRole.Id))
                    return new QuiccbanFailResult(string.Format(_responseService.Get("user_already_muted"), u.ToString(), u.Mention));
                else
                {
                    var cases = dbGuild.Cases.Where(x => !x.Resolved && x.ActionType == ActionType.TempMute && x.TargetId == u.Id);

                    foreach (var @case in cases)
                    {
                        await _databaseService.ResolveCaseAsync(@case, Context.Guild.CurrentUser, "Case not in-sync.");
                    }
                }


                await u.AddRoleAsync(muteRole);

                await _databaseService.CreateNewCaseAsync(Context.Guild, reason, ActionType.TempMute, (int)time.TotalSeconds, Context.User, u);
            }
            catch (InvalidOperationException ex)
            {
                return new QuiccbanFailResult(ex.Message);
            }
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("tempmute_success"), u.ToString(), u.Mention, time.Humanize(4)));
        }

        [Command("mute")]
        [RequireUserPermission(GuildPermission.DeafenMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task<CommandResult> MuteAsync([RequireHigherOrEqualHierarchy, RequireHigherOrEqualBotHierarchy]SocketGuildUser u, [Remainder]string reason = null)
        {
            var dbGuild = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

            try
            {
                IRole muteRole = Context.Guild.GetRole(dbGuild.MuteRoleId);
                if (muteRole == null)
                    muteRole = await _helperService.CreateMuteRoleAsync(dbGuild);

                await u.AddRoleAsync(muteRole);

                if (u.Roles.Any(x => x.Id == muteRole.Id))
                    return new QuiccbanFailResult(string.Format(_responseService.Get("user_already_muted"), u.ToString(), u.Mention));
                else
                {
                    var cases = dbGuild.Cases.Where(x => !x.Resolved && x.ActionType == ActionType.TempMute && x.TargetId == u.Id);

                    foreach (var @case in cases)
                    {
                        await _databaseService.ResolveCaseAsync(@case, Context.Guild.CurrentUser, "Case not in-sync.");
                    }
                }

                await _databaseService.CreateNewCaseAsync(Context.Guild, reason, ActionType.Mute, 0, Context.User, u);
            }
            catch (InvalidOperationException ex)
            {
                return new QuiccbanFailResult(ex.Message);
            }
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("mute_success"), u.ToString(), u.Mention));
        }

        [Command("unmute")]
        [RequireUserPermission(GuildPermission.DeafenMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task<CommandResult> UnmuteAsync([RequireHigherOrEqualHierarchy, RequireHigherOrEqualBotHierarchy]SocketGuildUser u, [Remainder]string reason = null)
        {
            var dbGuild = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

            try
            {
                IRole muteRole = Context.Guild.GetRole(dbGuild.MuteRoleId);
                if (muteRole == null)
                    muteRole = await _helperService.CreateMuteRoleAsync(dbGuild);

                var @case = dbGuild.Cases.LastOrDefault(x => !x.Resolved && (x.ActionType == ActionType.TempMute || x.ActionType == ActionType.Mute) && x.TargetId == u.Id);

                if (@case == null)
                    return new QuiccbanFailResult(string.Format(_responseService.Get("unmute_not_muted"), u.ToString(), u.Mention));

                await _databaseService.ResolveCaseAsync(@case, Context.User, reason);
            }
            catch (InvalidOperationException ex)
            {
                return new QuiccbanFailResult(ex.Message);
            }

            return new QuiccbanSuccessResult(string.Format(_responseService.Get("unmute_success"), u.ToString(), u.Mention));
        }

        [Command("kick")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task<CommandResult> KickAsync([RequireHigherHierarchy, RequireHigherBotHierarchy]SocketGuildUser u, [Remainder]string reason = null)
        {
            await u.KickAsync(reason);
            try
            {
                await _databaseService.CreateNewCaseAsync(Context.Guild, reason, ActionType.Kick, 0, Context.User, u);
            }
            catch (InvalidOperationException ex)
            {
                return new QuiccbanFailResult(ex.Message);
            }
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("kick_success"), u.ToString(), u.Mention));
        }

        [Command("tempban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task<CommandResult> TempBanAsync([RequireHigherHierarchy, RequireHigherBotHierarchy]SocketGuildUser u, TimeSpan time, [Remainder]string reason = null)
        {
            await u.BanAsync(0, reason);
            try
            {
                await _databaseService.CreateNewCaseAsync(Context.Guild, reason, ActionType.TempBan, (int)time.TotalSeconds, Context.User, u);
            }
            catch (InvalidOperationException ex)
            {
                return new QuiccbanFailResult(ex.Message);
            }
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("tempban_success"), u.ToString(), u.Mention, time.Humanize(4)));
        }

        [Command("ban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task<CommandResult> BanAsync([RequireHigherHierarchy, RequireHigherBotHierarchy]SocketGuildUser u, [Remainder]string reason = null)
        {
            await u.BanAsync(0, reason);
            try
            {
                await _databaseService.CreateNewCaseAsync(Context.Guild, reason, ActionType.Ban, 0, Context.User, u);
            }

            catch (InvalidOperationException ex)
            {
                return new QuiccbanFailResult(ex.Message);
            }
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("ban_success"), u.ToString(), u.Mention));
        }

        [Command("hackban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task<CommandResult> HackbanAsync(ulong u, [Remainder]string reason = null)
        {
            var user = (IUser)await ((IGuild)Context.Guild).GetUserAsync(u);
            if (user != null)
                return new QuiccbanFailResult(string.Format(_responseService.Get("hackban_user_in_guild"), user.ToString(), user.Mention));

            user = await Context.Client.Rest.GetUserAsync(u);

            try
            {
                await Context.Guild.AddBanAsync(user, reason: reason);
            }
            catch (HttpException ex)
            {
                if (ex.HttpCode == System.Net.HttpStatusCode.NotFound)
                    return new QuiccbanFailResult(string.Format(_responseService.Get("hackban_user_not_found"), u));

                return new QuiccbanFailResult(string.Format(_responseService.Get("hackban_fail")));
            }
            try
            {
                await _databaseService.CreateNewCaseAsync(Context.Guild, reason, ActionType.HackBan, 0, Context.User, user);
            }
            catch (InvalidOperationException ex)
            {
                return new QuiccbanFailResult(ex.Message);
            }
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("hackban_success"), u));
        }


        [Command("unban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task<CommandResult> UnbanAsync(RestBan ban, [Remainder]string reason = null)
        {
            await Context.Guild.RemoveBanAsync(ban.User);
            try
            {
                await _databaseService.CreateNewCaseAsync(Context.Guild, reason, ActionType.Unban, 0, Context.User, ban.User);
            }
            catch (InvalidOperationException ex)
            {
                return new QuiccbanFailResult(ex.Message);
            }
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("unban_success"), ban.User.ToString(), ban.User.Mention));
        }


        [Command("clean")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public Task<CommandResult> CleanAsync(SocketGuildUser u, int quota = 100, [Remainder] CleanType[] types = null)
            => CleanAsync(u.Id, quota, types);

        [Command("clean")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public Task<CommandResult> CleanAsync(int quota, SocketGuildUser u = null, [Remainder] CleanType[] types = null)
            => CleanAsync(u == null ? 0 : u.Id, quota, types);

        [Command("clean")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public Task<CommandResult> CleanAsync(int quota, [Remainder] CleanType[] types = null)
            => CleanAsync(0, quota, types);

        [Command("clean")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public Task<CommandResult> CleanAsync(CleanType type, int quota = 100, SocketGuildUser u = null)
            => CleanAsync(u == null ? 0 : u.Id, quota, new CleanType[] { type });


        public async Task<CommandResult> CleanAsync(ulong u, int quota, CleanType[] types)
        {
            if (quota > 300)
                return new QuiccbanFailResult(_responseService.Get("clean_amount_too_large"));

            types = types ?? new CleanType[] { };

            var distinctedTypes = types.Distinct();

            var messages = new HashSet<IUserMessage>();

            int attemptCount = 0;
            ulong lastMessageId = 0;

            grabMessages:
            int getAmount = quota - messages.Count;
            var unfilteredMessages = (lastMessageId == 0 ? await Context.Channel.GetMessagesAsync(Context.Message.Id, Direction.Before, getAmount).FlattenAsync() : await Context.Channel.GetMessagesAsync(lastMessageId, Direction.Before, getAmount).FlattenAsync());
            lastMessageId = unfilteredMessages.LastOrDefault() == null ? lastMessageId : unfilteredMessages.LastOrDefault().Id;
            var channelMessages = unfilteredMessages.Where(x => x is IUserMessage && (DateTimeOffset.UtcNow - x.CreatedAt).TotalDays < 14).Cast<IUserMessage>();
            if (u != 0)
                channelMessages.Where(x => x.Author.Id == u);

            _helperService.FilterCleaningCollection(ref channelMessages, types);
            foreach (var message in channelMessages)
            {
                if (!messages.Any(x => x.Id == message.Id))
                {
                    messages.Add(message);
                }
                else
                {
                    attemptCount = 5;
                    break;
                }
            }

            if (attemptCount < 5 && messages.Count < quota && channelMessages.Count() != 0)
            {
                attemptCount++;
                goto grabMessages;
            }


            if (messages.Count == 0)
                return new QuiccbanFailResult(_responseService.Get("clean_no_messages"));


            await Context.Channel.DeleteMessagesAsync(messages.Take(quota));
            return new QuiccbanSuccessResult(string.Format(_responseService.Get("clean_success"), messages.Take(quota).Count()));
        }


    }
}
