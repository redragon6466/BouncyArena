using Assets.Services.Containers;
using Assets.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Services
{
    public class BettingService : IBettingService
    {
        private List<Bet> TeamOneBets = new List<Bet>();
        private List<Bet> TeamTwoBets = new List<Bet>();
        private int TeamOnePool = 0;
        private int TeamTwoPool = 0;



        private static BettingService instance = null;
        private static readonly object padlock = new object();
        private static readonly object teamOnePoolLock = new object();
        private static readonly object teamTwoPoolLock = new object();

        BettingService()
        {
        }

        public static BettingService Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new BettingService();
                    }
                    return instance;
                }
            }
        }

        public void AddBetToTeam(string viewername, int amount, int team)
        {
            Debug.Log("Add bet to team: " + viewername + ", " + amount +", " +team);
            if (team == 1)
            {
                var bet = new Bet(viewername, amount);
                lock (teamOnePoolLock)
                {
                    TeamOneBets.Add(bet);
                    TeamOnePool += amount;
                }
            }
            if (team == 2)
            {
                var bet = new Bet(viewername, amount);
                lock (teamTwoPoolLock)
                {
                    TeamTwoBets.Add(bet);
                    TeamTwoPool += amount;
                }
            }
        }

        

        public void StartNewRound()
        {
            lock (teamOnePoolLock)
            {
                TeamOneBets.Clear();
                TeamOnePool = 1000;
            }
            lock (teamTwoPoolLock)
            {
                TeamTwoBets.Clear();
                TeamTwoPool = 1000;
            }
        }

        public List<Tuple<string, int>> PayoutBets(int winningTeam)
        {
            Debug.Log("Pay Bets");
            var winning = new List<Tuple<string, int>>();
            var pool = 0;
            var pot = TeamOnePool + TeamTwoPool;
            Debug.Log(pot);
            var bets = new List<Bet>();
            if (winningTeam == 1)
            {
                lock (teamOnePoolLock)
                {
                    pool = TeamOnePool;
                    bets = TeamOneBets;
                }
            }
            if (winningTeam == 2)
            {
                lock (teamTwoPoolLock)
                {
                    pool = TeamTwoPool;
                    bets = TeamTwoBets;
                }
                
            }

            Debug.Log(bets.Count);
            foreach (var bet in bets)
            {
                var payout = (int)((float)bet.Amount / pool * pot);
                Debug.Log(payout);
                winning.Add(new Tuple<string, int>(bet.ViewerName, payout));
            }

            return winning;
        }

        public List<Bet> GetTeamOneBets()
        {
            var returned = new List<Bet>();
            lock (teamOnePoolLock)
            {
                returned = new List<Bet>(TeamOneBets);
            }
            return returned;
        }

        public List<Bet> GetTeamTwoBets()
        {
            var returned = new List<Bet>();
            lock (teamTwoPoolLock)
            {

                returned = new List<Bet>(TeamTwoBets);
            }
            return returned;
        }
    }
}
