using quiccban.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.API.Entities
{
    public class PartialDatabaseGuild
    {
        public PartialDatabaseGuild(Guild guild)
        {
            Id = guild.Id.ToString();
            MuteRoleId = guild.MuteRoleId.ToString();
            ModlogChannelId = guild.ModlogChannelId.ToString();
            LogStyle = guild.LogStyle;
            WarnThreshold = guild.WarnThreshold;
            WarnExpiry = guild.WarnExpiry;
            WarnThresholdActionType = guild.WarnThresholdActionType;
            WarnThresholdActionExpiry = guild.WarnThresholdActionExpiry;
            AutoMod = guild.AutoMod;
        }

        public string Id;
        public string MuteRoleId;
        public string ModlogChannelId;
        public LogStyle LogStyle;
        public int WarnThreshold;
        public int WarnExpiry;
        public ActionType WarnThresholdActionType;
        public int WarnThresholdActionExpiry;
        public AutoMod AutoMod;

    }
}
