using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace quiccban.Services.Discord.Commands
{
    /// <summary>
    ///     Requires the bot to have a specific permission in the channel a command is invoked in.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireBotPermissionAttribute : CheckBaseAttribute
    {
        /// <summary>
        ///     Gets the specified <see cref="Discord.GuildPermission" /> of the precondition.
        /// </summary>
        public GuildPermission? GuildPermission { get; }
        /// <summary>
        ///     Gets the specified <see cref="Discord.ChannelPermission" /> of the precondition.
        /// </summary>
        public ChannelPermission? ChannelPermission { get; }
        /// <summary>
        ///     Gets or sets the error message if the precondition
        ///     fails due to being run outside of a Guild channel.
        /// </summary>
        public string NotAGuildErrorMessage { get; set; }

        /// <summary>
        ///     Requires the bot account to have a specific <see cref="Discord.GuildPermission"/>.
        /// </summary>
        /// <remarks>
        ///     This precondition will always fail if the command is being invoked in a <see cref="IPrivateChannel"/>.
        /// </remarks>
        /// <param name="permission">
        ///     The <see cref="Discord.GuildPermission"/> that the bot must have. Multiple permissions can be specified
        ///     by ORing the permissions together.
        /// </param>
        public RequireBotPermissionAttribute(GuildPermission permission)
        {
            GuildPermission = permission;
            ChannelPermission = null;
        }
        /// <summary>
        ///     Requires that the bot account to have a specific <see cref="Discord.ChannelPermission"/>.
        /// </summary>
        /// <param name="permission">
        ///     The <see cref="Discord.ChannelPermission"/> that the bot must have. Multiple permissions can be
        ///     specified by ORing the permissions together.
        /// </param>
        public RequireBotPermissionAttribute(ChannelPermission permission)
        {
            ChannelPermission = permission;
            GuildPermission = null;
        }

        public override Task<CheckResult> CheckAsync(ICommandContext ctx, IServiceProvider provider)
        {
            var context = (QuiccbanContext)ctx;

            var responseService = provider.GetService<ResponseService>();

            var guildUser = context.Guild.CurrentUser;


            if (GuildPermission.HasValue)
            {
                if (!guildUser.GuildPermissions.Has(GuildPermission.Value))
                    return Task.FromResult(new CheckResult(string.Format(responseService.Get("bot_require_guild_permission"), GuildPermission.Value)));
            }

            if (ChannelPermission.HasValue)
            {
                ChannelPermissions perms;
                if (context.Channel is IGuildChannel guildChannel)
                    perms = guildUser.GetPermissions(guildChannel);
                else
                    perms = ChannelPermissions.All(context.Channel);

                if (!perms.Has(ChannelPermission.Value))
                    return Task.FromResult(new CheckResult(string.Format(responseService.Get("bot_require_channel_permission"), ChannelPermission.Value)));
            }

            return Task.FromResult(CheckResult.Successful);
        }
    }
}