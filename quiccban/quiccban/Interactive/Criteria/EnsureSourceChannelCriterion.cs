using System.Threading.Tasks;
using Qmmands;
using quiccban.Services.Discord.Commands;

namespace Discord.Addons.Interactive
{
    public class EnsureSourceChannelCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(QuiccbanContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.Channel.Id == parameter.Channel.Id;
            return Task.FromResult(ok);
        }
    }
}
