using Qmmands;
using System;
using System.Collections.Generic;
using System.Text;

namespace quiccban.Services.Discord.Commands
{
    public sealed class QuiccbanFailResult : CommandResult
    {
        public string Reason { get; }
        public override bool IsSuccessful => false;

        public QuiccbanFailResult(string reason)
        {
            Reason = reason;
        }

    }
}
