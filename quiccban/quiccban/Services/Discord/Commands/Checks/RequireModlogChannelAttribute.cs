using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Humanizer;

namespace quiccban.Services.Discord.Commands
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireModlogChannelAttribute : CheckBaseAttribute
    {
        public override async Task<CheckResult> CheckAsync(ICommandContext ctx, IServiceProvider services)
        {
            var context = (QuiccbanContext)ctx;

            var dbService = services.GetService<DatabaseService>();
            var responseService = services.GetService<ResponseService>();
            var config = services.GetService<Config>();

            var dbGuild = await dbService.GetOrCreateGuildAsync(context.Guild);

            if (dbGuild.ModlogChannelId == 0)
                return new CheckResult(string.Format(responseService.Get("require_modlog_channel"), config.Prefix));

            var channel = context.Guild.GetTextChannel(dbGuild.ModlogChannelId);

            if(channel == null)
                return new CheckResult(string.Format(responseService.Get("modlog_channel_doesnt_exist")));

            ChannelPermissions perms = context.Guild.CurrentUser.GetPermissions(channel);

            if(!perms.Has(ChannelPermission.SendMessages))
                return new CheckResult(string.Format(responseService.Get("require_modlog_channel_permission"), channel.Name, channel.Mention, ChannelPermission.SendMessages.Humanize()));

            if (!perms.Has(ChannelPermission.ViewChannel))
                return new CheckResult(string.Format(responseService.Get("require_modlog_channel_permission"), channel.Name, channel.Mention, ChannelPermission.ViewChannel.Humanize()));

            if (!perms.Has(ChannelPermission.ReadMessageHistory))
                return new CheckResult(string.Format(responseService.Get("require_modlog_channel_permission"), channel.Name, channel.Mention, ChannelPermission.ReadMessageHistory.Humanize()));

            if (!perms.Has(ChannelPermission.EmbedLinks))
                return new CheckResult(string.Format(responseService.Get("require_modlog_channel_permission"), channel.Name, channel.Mention, ChannelPermission.EmbedLinks.Humanize()));

            return CheckResult.Successful;
        }
    }
}
