using Qmmands;
using System;
using System.Collections.Generic;
using System.Text;

namespace quiccban.Services.Discord.Commands
{
    public sealed class QuiccbanSuccessResult : CommandResult
    {
        public override bool IsSuccessful => true;

        public string ReplyValue { get; }

        public QuiccbanSuccessResult(string replyvalue = null)
        {
            ReplyValue = replyvalue;
        }
    }
}
