using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quiccban.Database.Models
{
    public class Guild
    {
        public ulong Id { get; set; }

        public ulong MuteRoleId { get; set; }
        public ulong ModlogChannelId { get; set; }

        public LogStyle LogStyle { get; set; }

        /// <summary>
        /// Amount of amount of warns before <see cref="WarnThresholdActionType"/> fires
        /// </summary>
        public int WarnThreshold { get; set; }
        /// <summary>
        /// Warn expiry in seconds.
        /// </summary>
        public int WarnExpiry { get; set; }
        public ActionType WarnThresholdActionType { get; set; }
        public int WarnThresholdActionExpiry { get; set; }

        public AutoMod AutoMod { get; set; }


        public List<Case> Cases { get; set; }
        
    }


    
    public class Case
    { 
        public int Id { get; set; }

        public int TiedTo { get; set; }

        public ulong DiscordMessageId { get; set; }

        public Guild Guild { get; set; }
        public ulong GuildId { get; set; }

        public ActionType ActionType { get; set; }
        public int ActionExpiry { get; set; }
        public bool Resolved { get; set; }
        public bool ForceResolved { get; set; }
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

        public DateTimeOffset GetWarnEndingTime()
            => DateTimeOffset.FromUnixTimeMilliseconds(UnixTimestamp).AddSeconds(Guild.WarnExpiry);

        public string GetDiscordMessageLink()
        {
            if (DiscordMessageId == 0)
                return null;

            return $"https://discordapp.com/channels/{GuildId}/{Guild.ModlogChannelId}/{DiscordMessageId}";
        }

    }

    public class AutoMod
    { 

        public bool Enabled { get; set; }

        public bool SpamEnabled { get; set; }
        public bool RaidEnabled { get; set; }

    }



    public enum ActionType
    {
        Warn,
        Kick,
        TempMute,
        Mute,
        TempBan,
        Ban,
        HackBan,
        Unwarn,
        Unmute,
        Unban,
        None
    }

    public enum LogStyle
    {
        Modern,
        Basic
    }
}
