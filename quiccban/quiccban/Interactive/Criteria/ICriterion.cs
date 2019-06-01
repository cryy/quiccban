using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using System.Threading.Tasks;
using Qmmands;
using quiccban.Services.Discord.Commands;

namespace Discord.Addons.Interactive
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(QuiccbanContext sourceContext, T parameter);
    }
}
