using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.Database.Models
{
    public class Guild
    {
        public ulong Id { get; set; }

        public ulong LogChannel { get; set; }

        /// <summary>
        /// Amount of amount of warns before <see cref="WarnThresholdActionType"/> fires
        /// </summary>
        public int WarnThreshold { get; set; }
        /// <summary>
        /// Warn expiry in seconds.
        /// </summary>
        public ulong WarnExpiry { get; set; }
        public ActionType WarnThresholdActionType { get; set; }
        public ulong WarnThresholdActionExpiry { get; set; }

        public bool AutoModEnabled { get; set; }

        //AMS = AutoModSpam
        public bool AMSEnabled { get; set; }
        public int AMSMessageTriggerAmount { get; set; }
        public ActionType AMSActionType { get; set; }
        public ulong AMSActionExpiry { get; set; }


        public List<Case> Cases { get; set; }
        
    }


    
    public class Case
    { 
        public int Id { get; set; }

        public Guild Guild { get; set; }
        public ulong GuildId { get; set; }

        public ActionType ActionType { get; set; }
        public ulong ActionExpiry { get; set; }
        public bool Resolved { get; set; }
        /// <summary>
        /// User who the action had been performed on.
        /// </summary>
        public ulong TargetId { get; set; }
        /// <summary>
        /// Action executor.
        /// </summary>
        public ulong IssuerId { get; set; }

        public string Reason { get; set; }

        /// <summary>
        /// Unix timestamp of occurance in milliseconds.
        /// </summary>
        public long UnixTimestamp { get; set; }

        public DateTimeOffset GetEndingTime()
            => DateTimeOffset.FromUnixTimeMilliseconds(UnixTimestamp).AddSeconds(ActionExpiry);

    }



    public enum ActionType
    {
        Warn,
        Kick,
        Tempmute,
        Mute,
        Tempban,
        Ban,
        Unwarn,
        Unmute,
        Unban,
        None
    }
}
