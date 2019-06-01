using System.Threading.Tasks;
using Qmmands;
using Discord.WebSocket;
using quiccban.Services.Discord.Commands;

namespace Discord.Addons.Interactive
{
    internal class EnsureReactionFromSourceUserCriterion : ICriterion<SocketReaction>
    {
        public Task<bool> JudgeAsync(QuiccbanContext sourceContext, SocketReaction parameter)
        {
            bool ok = parameter.UserId == sourceContext.User.Id;
            return Task.FromResult(ok);
        }
    }
}
