using System.Threading.Tasks;
using Qmmands;
using Discord.WebSocket;
using quiccban.Services.Discord.Commands;

namespace Discord.Addons.Interactive
{
    internal class EnsureIsIntegerCriterion : ICriterion<SocketMessage>
    {
        public Task<bool> JudgeAsync(QuiccbanContext sourceContext, SocketMessage parameter)
        {
            bool ok = int.TryParse(parameter.Content, out _);
            return Task.FromResult(ok);
        }
    }
}
