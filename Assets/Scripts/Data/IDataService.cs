using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Data
{
    public interface IDataService
    {

        bool CheckDatabase();
        bool CreateDatabase();
        Viewer GetViewer(string viewerName);
        bool UpdateBalance(string viewerName, int amount);
        bool UpdateSubscriberTier(string viewerName, SubscriptionTierEnum newTier);
        bool UpdateBitsSpent(string viewerName, int amount);
        bool UpdateChannelPointsSpent(string viewerName, int amount);
        bool UpdateFollowerStatus(string viewerName, bool isFollower);
        bool NewViewer(string viewerName);
        int GetBalance(string viewerName);
        string GetSubscriberLevel(string viewerName);
        DateTime GetLastLogin(string viewerName);

    }
}
