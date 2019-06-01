using System.Threading.Tasks;
using Qmmands;
using quiccban.Services.Discord.Commands;

namespace Discord.Addons.Interactive
{
    public class EnsureSourceUserCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(QuiccbanContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.User.Id == parameter.Author.Id;
            return Task.FromResult(ok);
        }
    }
}
