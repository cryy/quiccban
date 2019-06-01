using Qmmands;
using System;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Humanizer;

namespace quiccban.Services.Discord.Commands
{
    /// <summary>
    /// This attribute requires that the user invoking the command has a specified permission.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireUserPermissionAttribute : CheckBaseAttribute
    {
        /// <summary>
        ///     Gets the specified <see cref="Discord.GuildPermission" /> of the precondition.
        /// </summary>
        public GuildPermission? GuildPermission { get; }
        /// <summary>
        ///     Gets the specified <see cref="Discord.ChannelPermission" /> of the precondition.
        /// </summary>
        public ChannelPermission? ChannelPermission { get; }
        /// <inheritdoc />
        public string NotAGuildErrorMessage { get; set; }

        /// <summary>
        ///     Requires that the user invoking the command to have a specific <see cref="Discord.GuildPermission"/>.
        /// </summary>
        /// <remarks>
        ///     This precondition will always fail if the command is being invoked in a <see cref="IPrivateChannel"/>.
        /// </remarks>
        /// <param name="permission">
        ///     The <see cref="Discord.GuildPermission" /> that the user must have. Multiple permissions can be
        ///     specified by ORing the permissions together.
        /// </param>
        public RequireUserPermissionAttribute(GuildPermission permission)
        {
            GuildPermission = permission;
            ChannelPermission = null;
        }
        /// <summary>
        ///     Requires that the user invoking the command to have a specific <see cref="Discord.ChannelPermission"/>.
        /// </summary>
        /// <param name="permission">
        ///     The <see cref="Discord.ChannelPermission"/> that the user must have. Multiple permissions can be
        ///     specified by ORing the permissions together.
        /// </param>
        public RequireUserPermissionAttribute(ChannelPermission permission)
        {
            ChannelPermission = permission;
            GuildPermission = null;
        }

        /// <inheritdoc />
        public override Task<CheckResult> CheckAsync(ICommandContext ctx, IServiceProvider services)
        {

            var context = (QuiccbanContext)ctx;

            var responseService = services.GetService<ResponseService>();

            var guildUser = context.User;

            if (GuildPermission.HasValue)
            {
                if (!guildUser.GuildPermissions.Has(GuildPermission.Value))
                    return Task.FromResult(new CheckResult(string.Format(responseService.Get("require_guild_permission"), GuildPermission.Value.Humanize())));
            }

            if (ChannelPermission.HasValue)
            {
                ChannelPermissions perms;
                if (context.Channel is IGuildChannel guildChannel)
                    perms = guildUser.GetPermissions(guildChannel);
                else
                    perms = ChannelPermissions.All(context.Channel);

                if (!perms.Has(ChannelPermission.Value))
                    return Task.FromResult(new CheckResult(string.Format(responseService.Get("require_channel_permission"), ChannelPermission.Value.Humanize())));
            }

            return Task.FromResult(CheckResult.Successful);
        }
    }
}
