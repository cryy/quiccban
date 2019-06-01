using System;
using System.Threading.Tasks;
using Qmmands;
using Discord.WebSocket;
using quiccban.Services.Discord.Commands;

namespace Discord.Addons.Interactive
{
    public interface IReactionCallback
    {
        RunMode RunMode { get; }
        ICriterion<SocketReaction> Criterion { get; }
        TimeSpan? Timeout { get; }
        QuiccbanContext Context { get; }

        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}
