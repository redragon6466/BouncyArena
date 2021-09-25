using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public class UserTriggeredService 
    {
        List<string> _activeWeather = new List<string>();
        ArenaData _activeArena;
        private object _activeEventsLock = new object();


        #region Instance
        private static UserTriggeredService instance = null;
        private static readonly object padlock = new object();
        private static readonly object teamOnePoolLock = new object();
        private static readonly object teamTwoPoolLock = new object();

        UserTriggeredService()
        {
        }

        public static UserTriggeredService Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new UserTriggeredService();
                    }
                    return instance;
                }
            }
        }

        #endregion

        public bool WeatherRock(string symptom)
        {
            if (UserTriggeredEventsEnum.Blizzard.ToString().ToLower().Equals(symptom.ToLower()))
            {

            }
            return false;
        }

        public void SetArena(ArenaData arenaData)
        {
            _activeArena = arenaData;
        }

        public void TriggerWeather(string symptom)
        {
            if (UserTriggeredEventsEnum.Blizzard.ToString().ToLower().Equals(symptom.ToLower()))
            {
                var test = _activeArena.ArenaName;
               var snowName = test + "Snow";
                var snow = GameObject.FindObjectsOfType(typeof(ParticleSystem)).ToList().Where(x => x.name.Equals(snowName)).FirstOrDefault();
                if (snow!= null)
                {
                    ((ParticleSystem)snow).Play();
                }

                Physics.gravity = new Vector3(0, -27.0F, 0);
                _activeWeather.Add(UserTriggeredEventsEnum.Blizzard.ToString());
            }

        }

        public void EndWeather(string symptom)
        {
            if (UserTriggeredEventsEnum.Blizzard.ToString().ToLower().Equals(symptom.ToLower()))
            {
                var snowName = _activeArena.ArenaName + "Snow";
                var snow = GameObject.FindObjectsOfType(typeof(ParticleSystem)).ToList().Where(x => x.name.Equals(snowName)).FirstOrDefault();
                if (snow != null)
                {
                    ((ParticleSystem)snow).Stop();
                }
                Physics.gravity = new Vector3(0, -9.0F, 0);

                lock (_activeEventsLock)
                {
                    _activeWeather.Remove(UserTriggeredEventsEnum.Blizzard.ToString());
                }
               
            }

        }

        public bool AlreadyGoing(string symptom)
        {
            var result = true;
            lock (_activeEventsLock)
            {
                result = _activeWeather.Contains(symptom.ToLower());
            }

            return result;
        }

        public List<string> GetActiveEvents()
        {
            List<string> result;
            lock (_activeEventsLock)
            {
                result = _activeWeather;
            }

            return result;

        }
    }
}
