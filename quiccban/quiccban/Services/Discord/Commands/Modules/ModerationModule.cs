using Discord;
using Discord.WebSocket;
using Qmmands;
using quiccban.Database.Models;
using quiccban.Services.Discord.Commands.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActionType = quiccban.Database.Models.ActionType;
using Humanizer;
using Discord.Rest;

namespace quiccban.Services.Discord.Commands.Modules
{
    [RequireLogChannel]
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
        public async Task SetReasonAsync(Case @case, [Remainder]string reason)
        {
            if(@case.IssuerId != Context.User.Id)
            {
                await ReplyAsync(_responseService.Get("isnt_case_owner"));
                return;
            }

            @case.Reason = reason;

            await _databaseService.UpdateCaseAsync(@case);
            await _caseHandlingService.UpdateDiscordMessage(@case);

            await ReplyAsync(string.Format(_responseService.Get("reason_success"), @case.Id));
        }

        [Command("warn")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task WarnAsync([RequireHigherOrEqualHierarchy]SocketGuildUser u, [Remainder]string reason = null)
        {
            await _databaseService.CreateNewCaseAsync(Context.Guild, reason, ActionType.Warn, 0, Context.User.Id, u.Id);
            await ReplyAsync(string.Format(_responseService.Get("warn_success"), u.ToString(), u.Mention));
        }

        [Command("tempmute")]
        [RequireUserPermission(GuildPermission.DeafenMembers)]
        public async Task TempMuteAsync([RequireHigherOrEqualHierarchy]SocketGuildUser u, TimeSpan time, [Remainder]string reason = null)
        {
            var dbGuild = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

            IRole muteRole = Context.Guild.GetRole(dbGuild.MuteRoleId);
            if (muteRole == null)
                muteRole = await _helperService.CreateMuteRoleAsync(dbGuild);

            if(u.Roles.Any(x => x.Id == muteRole.Id))
            {
                await ReplyAsync(string.Format(_responseService.Get("user_already_muted"), u.ToString(), u.Mention));
                return;
            }


            await u.AddRoleAsync(muteRole);

            await _databaseService.CreateNewCaseAsync(Context.Guild, reason, ActionType.Tempmute, (int)time.TotalSeconds, Context.User.Id, u.Id);
            await ReplyAsync(string.Format(_responseService.Get("tempmute_success"), u.ToString(), u.Mention, time.Humanize(4)));
        }

        [Command("mute")]
        [RequireUserPermission(GuildPermission.DeafenMembers)]
        public async Task TempMuteAsync([RequireHigherOrEqualHierarchy]SocketGuildUser u, [Remainder]string reason = null)
        {
            var dbGuild = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

            IRole muteRole = Context.Guild.GetRole(dbGuild.MuteRoleId);
            if (muteRole == null)
                muteRole = await _helperService.CreateMuteRoleAsync(dbGuild);

            await u.AddRoleAsync(muteRole);

            if (u.Roles.Any(x => x.Id == muteRole.Id))
            {
                await ReplyAsync(string.Format(_responseService.Get("user_already_muted"), u.ToString(), u.Mention));
                return;
            }

            await _databaseService.CreateNewCaseAsync(Context.Guild, reason, ActionType.Mute, 0, Context.User.Id, u.Id);
            await ReplyAsync(string.Format(_responseService.Get("mute_success"), u.ToString(), u.Mention));
        }

        [Command("tempban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task TempBanAsync([RequireHigherHierarchy]SocketGuildUser u, TimeSpan time, [Remainder]string reason = null)
        {
            var dbGuild = await _databaseService.GetOrCreateGuildAsync(Context.Guild);
            
            await u.BanAsync(0, reason);

            await _databaseService.CreateNewCaseAsync(Context.Guild, reason, ActionType.Tempban, (int)time.TotalSeconds, Context.User.Id, u.Id);
            await ReplyAsync(string.Format(_responseService.Get("tempmute_success"), u.ToString(), u.Mention, time.Humanize(4)));
        }

    }
}
