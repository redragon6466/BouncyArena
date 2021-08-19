using Assets.Services.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Services.Interfaces
{
    public interface IBettingService
    {
        List<Bet> GetTeamOneBets();
        List<Bet> GetTeamTwoBets();

        void AddBetToTeam(string viewername, int amount, int team);

        void PayoutBets(int winningTeam);

        void StartNewRound();


    }
}
