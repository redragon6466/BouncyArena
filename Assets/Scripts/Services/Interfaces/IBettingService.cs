using Assets.Scripts.Data;
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


        void PayoutBets(int winningTeam);

        void StartNewRound();

        PingResponse SendPing();
    }
}
