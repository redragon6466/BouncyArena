using Assets.Scripts.Data;
using Assets.Scripts.Services.Containers;
using Assets.Services.Containers;
using Assets.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        
        public void PayoutBets(int winningTeam)
        {
            Task.Factory.StartNew(() => SendHttpWinner(winningTeam));

        }

        void SendHttpWinner(int winningTeam)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format("https://localhost:44369/Home/PayoutBet?winningTeam={0}", winningTeam));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string str = reader.ReadLine();
            while (str != null)
            {
                Debug.Log(str);
                str = reader.ReadLine();
            }
        }

       public PingResponse SendPing()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://localhost:44369/Home/PingResponse");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string str = reader.ReadLine();
            PingResponse pingResponse = JsonConvert.DeserializeObject<PingResponse>(str);
            return pingResponse;
            }
        }

}
