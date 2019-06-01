using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Qmmands;
using Discord.WebSocket;
using quiccban.Services.Discord.Commands;

namespace Discord.Addons.Interactive
{
    public class EmptyCriterion<T> : ICriterion<T>
    {
        public Task<bool> JudgeAsync(QuiccbanContext sourceContext, T parameter)
            => Task.FromResult(true);
    }
}
