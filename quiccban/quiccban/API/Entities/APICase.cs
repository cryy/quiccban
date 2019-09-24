using quiccban.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API.Entities
{
    public class APICase
    {
        public APICase(Case @case, User targetUser = null, User issuerUser = null)
        {
            Id = @case.Id;
            DiscordMessageId = @case.DiscordMessageId.ToString();
            Guild = new PartialDatabaseGuild(@case.Guild);
            GuildId = Guild.Id;
            ActionType = @case.ActionType;
            ActionExpiry = @case.ActionExpiry;
            Resolved = @case.Resolved;
            ForceResolved = @case.ForceResolved;
            TargetId = @case.TargetId.ToString();
            IssuerId = @case.IssuerId.ToString();
            Reason = @case.Reason;
            UnixTimestamp = @case.UnixTimestamp.ToString();
            TargetUser = targetUser;
            IssuerUser = issuerUser;
        }

        public int Id { get; set; }
        public string DiscordMessageId { get; set; }
        public PartialDatabaseGuild Guild { get; set; }
        public string GuildId;
        public ActionType ActionType;
        public int ActionExpiry;
        public bool Resolved;
        public bool ForceResolved;
        public string TargetId;
        public string IssuerId;
        public User TargetUser;
        public User IssuerUser;
        public string Reason;
        public string UnixTimestamp;

    }
}
