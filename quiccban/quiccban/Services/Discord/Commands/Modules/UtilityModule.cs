using Discord;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Humanizer;
using Qmmands;
using quiccban.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActionType = quiccban.Database.Models.ActionType;

namespace quiccban.Services.Discord.Commands.Modules
{
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public class UtilityModule : QuiccbanModule<QuiccbanContext>
    {
        DatabaseService _databaseService;
        ResponseService _responseService;
        HelperService _helperService;
        InteractiveService _interactiveService;

        public UtilityModule(DatabaseService databaseService, ResponseService responseService, HelperService helperService, InteractiveService interactiveService)
        {
            _databaseService = databaseService;
            _responseService = responseService;
            _helperService = helperService;
            _interactiveService = interactiveService;
        }

        [Command("history")]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task<CommandResult> HistoryAsync(SocketGuildUser u, bool onlyActive = false, [IsntActionType(ActionType.None)]ActionType type = default)
        {
            var dbGuild = await _databaseService.GetOrCreateGuildAsync(Context.Guild);

            var history = type == default ? dbGuild.Cases.Where(x => x.TargetId == u.Id) : dbGuild.Cases.Where(x => x.TargetId == u.Id && x.ActionType == type);
            history = onlyActive ? history.Where(x => !x.Resolved) : history;

            if (!history.Any())
                return new QuiccbanFailResult(string.Format(_responseService.Get("history_no_cases"), u.ToString(), u.Mention, onlyActive ? "active " : ""));


            List<Embed> embeds = new List<Embed>();
            var historyGrouping = history.Select((@case, index) => new { Case = @case, Index = index }).GroupBy(x => x.Index / 5, x => x.Case);

            foreach (var group in historyGrouping)
                embeds.Add(await _helperService.ConstructHistoryEmbedAsync(group, u));

            if (embeds.Count > 1)
            {
                var paginatedMessage = new PaginatedMessage { Pages = embeds };

                var criterion = new Criteria<SocketReaction>();
                criterion.AddCriterion(new EnsureReactionFromSourceUserCriterion());

                await _interactiveService.SendPaginatedMessageAsync(Context, paginatedMessage, criterion);
            }
            else
                await ReplyAsync(embed: embeds.FirstOrDefault());

            return new QuiccbanSuccessResult();
        }

        [Command("history")]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public Task<CommandResult> HistoryAsync(SocketGuildUser u, [IsntActionType(ActionType.None)]ActionType type = default, bool onlyActive = false)
            => HistoryAsync(u, onlyActive, type);

        [Command("case")]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        public async Task d(Case @case)
        {
            switch (@case.Guild.LogStyle)
            {
                case LogStyle.Basic:
                    await ReplyAsync(await _helperService.ConstructCaseMessageAsync(@case));
                    break;
                case LogStyle.Modern:
                    var eb = new EmbedBuilder();
                    eb.WithTitle($"Case **{@case.Id}**  »  {@case.ActionType.Humanize()}");
                    eb.WithDescription(await _helperService.ConstructCaseMessageAsync(@case));
                    await ReplyAsync(embed: eb.Build());
                    break;
            }
        }
    }
}
