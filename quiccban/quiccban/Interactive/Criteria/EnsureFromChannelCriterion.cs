using System.Threading.Tasks;
using Qmmands;
using quiccban.Services.Discord.Commands;

namespace Discord.Addons.Interactive
{
    public class EnsureFromChannelCriterion : ICriterion<IMessage>
    {
        private readonly ulong _channelId;

        public EnsureFromChannelCriterion(IMessageChannel channel)
            => _channelId = channel.Id;

        public Task<bool> JudgeAsync(QuiccbanContext sourceContext, IMessage parameter)
        {
            bool ok = _channelId == parameter.Channel.Id;
            return Task.FromResult(ok);
        }
    }
}
