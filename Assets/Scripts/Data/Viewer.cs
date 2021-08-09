using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Data
{
    // headers = "id", "viewerName", "balance", "subscriberLevel", "isFollower", "bitsSpent", "channelPointsSpent", "lastInteraction"
    public class Viewer
    {
        public string ViewerId { get; set; }
        public string ViewerName { get; set; }
        public int Balance { get; set; }
        public SubscriptionTierEnum SubsriberLevel { get; set; }
        public bool IsFollower { get; set; }
        public int BitsSpent { get; set; }
        public int ChannelPointsSpent { get; set; }
        public DateTime? LastInteraction { get; set; }

        public string CreateInsertStatement(string dabaseName)
        {
            return string.Format("INSERT INTO {0}(viewerName, balance, subscriberLevel, isFollower, bitsSpent, channelPointsSpent, lastInteraction) VALUES('{1}', {2}, '{3}', '{4}', {5}, {6}, '{7}')", dabaseName, ViewerName, Balance, SubsriberLevel.ToString(), IsFollower.ToString(), BitsSpent, ChannelPointsSpent, LastInteraction.ToString());
        }
    }
}
