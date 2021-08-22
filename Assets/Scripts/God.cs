using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Data;
using Assets.Services;
using Assets.Scripts.Data;
using System.Threading;

namespace Assets
{
    public class God : MonoBehaviour
    {
        const string BlobStatsFormat = "Blob {0}\nHp: {1}\nAttack: {2}\nDefense: {3}";

        #region Serialized Fields

        [SerializeField]
        private List<Combatant> TeamOneBlobs;
        [SerializeField]
        private List<Combatant> TeamTwoBlobs;
        [SerializeField]
        GameObject[] blueBlobPrefab;
        [SerializeField]
        GameObject[] redBlobPrefab;

        [SerializeField]
        AudioClip[] BackgroudMusic;
        [SerializeField]
        AudioClip LineupMusic;

        #endregion

        #region Private Variables


        int[] _blueBlobSelectedPrefabIndex = new int[3];
        int[] _redBlobSelectedPrefabIndex = new int[3];

        private Text countDown;
        private float _vsTimer = 0.0f;
        private const int VsScreenTime = 11; //Add an extra 1 as the timer imediately starts at 10.9 which rounds down to 10, instead of 9.9 => 9
        private int _count = 0;
        private const float _vsScale = .25f;
        private const float _battleScale = .38f;
        private bool _fadeOutBegin = false;

        //private IBrain[] _teamOneBrains = { new GuardianBrain(), new GuardianBrain(), new HealerBrain(), };
        //private IClass[] _teamOneClasses = { new GuardianClass(), new GuardianClass(), new HealerClass(), };
        /*private IBrain[] _teamOneBrains = { new WarriorBrain() , new WarriorBrain(), new WarriorBrain(), };
        private IClass[] _teamOneClasses = { new WarriorClass() , new WarriorClass(), new WarriorClass(), };
        private IBrain[] _teamTwoBrains = { new HealerBrain(), new GuardianBrain(), new GuardianBrain(), };
        private IClass[] _teamTwoClasses = { new HealerClass(), new GuardianClass(), new GuardianClass(), };*/

        //Selected Positions

        private ArenaData _selectedArena;

        private ArenaData _debugArena;
        private List<ArenaData> _arenas;

        //LINEUP POSITIONS
        Vector2[] _blueStartPos = { new Vector2(-3.67f, 2.76f), new Vector2(-3.67f, -.1f), new Vector2(-3.67f, -3.5f), };
        Vector2[] _redStartPos = { new Vector2(3.55f, 2.91f), new Vector2(3.55f, -.1f), new Vector2(3.55f, -3.5f), };

        #endregion

        public God()
        {
            //instance = this;
        }

        #region Instance

        private static God instance = null;
        private static readonly object padlock = new object();


        public static God Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new God();
                    }
                    return instance;
                }
            }
        }

        #endregion

        #region Properties
        public bool IsBattling { get; private set; } = false;
        public Vector3 SelectedCameraPos { get => _selectedArena.CameraPos;  }
        public Vector3 SelectedCameraRot { get => _selectedArena.CameraRot; }

        #endregion

        #region Unity Methods

        // Start is called before the first frame update
        void Start()
        {
            instance = this;
            SetupArenas();
            StartVsScreen();

            //Task.Run(() => StartDatabaseManager());
            //Task.Run(() => StartTwitchBot());
        }

        public void Awake()
        {
            int gameStatusCount = FindObjectsOfType<God>().Length;
            if (gameStatusCount > 1)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject); //when the scene changes don't destroy the game object that owns this
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey("escape"))
            {

                Application.Quit();
            }

            if (!IsBattling)
            {
                var audio = GetComponent<AudioSource>();
                _vsTimer -= Time.deltaTime;
                if (countDown != null)
                {
                    countDown.text = Math.Floor(_vsTimer).ToString();
                    if (_vsTimer <= 0)
                    {
                        audio.volume = 0;
                        StartBattle();
                    }
                    else if (_vsTimer <= 3 && !_fadeOutBegin)
                    {
                        _fadeOutBegin = true;
                        StartCoroutine(StartFade(3, 0));
                    }
                }

                return;
            }
            else
            {
                if (UnityEngine.Random.Range(0, 20) > 18)
                {
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        TeamOneBlobs[UnityEngine.Random.Range(0, TeamOneBlobs.Count - 1)].SetTarget(TeamOneBlobs, TeamTwoBlobs, 1);
                    }
                    else
                    {
                        TeamTwoBlobs[UnityEngine.Random.Range(0, TeamTwoBlobs.Count - 1)].SetTarget(TeamOneBlobs, TeamTwoBlobs, 2);
                    }
                }
            }

        }

        public void OnDestroy()
        {
            
        }
        #endregion

        #region Public Methods
        public void StartVsScreen()
        {
            //Debug.Log("start vs");
            ChooseArena();
            var temp = GameObject.FindWithTag("CountDown");

            //Debug.Log(temp != null);
            if (temp != null)
            {
                //Debug.Log("found text");
                countDown = temp.GetComponent<Text>();
            }



            CreateLineup();
            StartCountdown();

            var audio = GetComponent<AudioSource>();
            audio.clip = LineupMusic;
            audio.Play();
            StartCoroutine(StartFade(3, .6f));


            //BettingService.Instance.StartNewRound();
            IsBattling = false;
        }

        public void StartBattle()
        {
            var scene = SceneManager.GetSceneByName(_selectedArena.SceneName);
            if (scene == null)
            {
                SceneManager.LoadScene(_debugArena.SceneName);
            }
            else
            {

                SceneManager.LoadScene(_selectedArena.SceneName);
            }

            //Task task = new Task(() => UpdateBalancesOnRoundStart());
            //task.Start();

            var audio = GetComponent<AudioSource>();
            audio.clip = _selectedArena.BackgroundMusic;
            audio.Play();
            StartCoroutine(StartFade(3, .6f));

            MoveTeams();
            IsBattling = true;


        }





        public void EndBattle()
        {
            IsBattling = false;
            _fadeOutBegin = false;
            _vsTimer = 0;

            //TODO kickoff music fade here
            var audio = GetComponent<AudioSource>();
            StartCoroutine(StartFade(3, 0));

            //add delay????
            StartCoroutine(DelayBytTimeCoroutine(3));

        }

        public void EndBattleTwoEletricbogaloo()
        {
            //delete all blobs and find winning team
            var temp = TeamOneBlobs.ToList();
            int team = 0;

            team = TeamOneBlobs.Count != 0 ? 1 : 2;

            var blobList = UnityEngine.Object.FindObjectsOfType(typeof(Combatant));

            foreach (var item in blobList)
            {
                var b = (Combatant)item;
                TeamOneBlobs.Remove(b);
                TeamTwoBlobs.Remove(b);

                b.OnDestroy();
                Destroy(b.gameObject);
            }


            //todo DRAW mechanics

            Debug.Log("Team " + team + " Wins!");
            Task task = new Task(() => UpdateBalancesOnRoundEnd(team));
            task.Start();

            SceneManager.LoadScene("Lineup");
            StartCoroutine(DelayOneFrame());
        }

        /// <summary>
        /// Gets a list of all the blobs
        /// </summary>
        /// <returns>a list of all blobs</returns>
        public List<Combatant> GetAllBlobs()
        {
            var blobs = new List<Combatant>(TeamOneBlobs);
            blobs.AddRange(TeamTwoBlobs);
            return blobs;
        }

        /// <summary>
        /// Gets team one
        /// </summary>
        /// <returns>a list of all blobs</returns>
        public List<Combatant> GetTeamOne()
        {
            var blobs = new List<Combatant>(TeamOneBlobs);
            return blobs;
        }

        /// <summary>
        /// Gets team one
        /// </summary>
        /// <returns>a list of all blobs</returns>
        public List<Combatant> GetTeamTwo()
        {
            var blobs = new List<Combatant>(TeamTwoBlobs);
            return blobs;
        }


        public void KillBlob(Combatant blob)
        {
            TeamOneBlobs.Remove(blob);
            TeamTwoBlobs.Remove(blob);

            if (TeamOneBlobs.Count == 0 || TeamTwoBlobs.Count == 0)
            {
                blob.OnDestroy();
                GameObject.Destroy(blob.gameObject);
                EndBattle();
                return;
            }

            foreach (var blobItem in TeamOneBlobs)
            {
                if (blobItem.ClassScript.target.GetComponent<Combatant>().ID == blob.ID)
                {
                    blobItem.SetTarget(TeamOneBlobs, TeamTwoBlobs, 1);
                }
            }
            foreach (var blobItem in TeamTwoBlobs)
            {
                if (blobItem.ClassScript.target.GetComponent<Combatant>().ID == blob.ID)
                {
                    blobItem.SetTarget(TeamOneBlobs, TeamTwoBlobs, 2);
                }
            }

            blob.OnDestroy();
            GameObject.Destroy(blob.gameObject);
        }
        #endregion

        #region Private Methods
        private string ChooseArena()
        {
            var map = UnityEngine.Random.Range(0, _arenas.Count);
            var arena = _arenas[map];
            _selectedArena = arena;
            return arena.SceneName;
        }

        private void CreateLineup()
        {
            TeamOneBlobs = new List<Combatant>();
            TeamOneBlobs.Clear();
            TeamTwoBlobs = new List<Combatant>();
            TeamTwoBlobs.Clear();

            var texts = FindObjectsOfType(typeof(Text)).ToList().OrderBy(x => ((Text)x).text);
            var stats = texts.ToList();
            stats.RemoveAt(0);

            for (int i = 0; i < 3; i++)
            {
                _blueBlobSelectedPrefabIndex[i] = UnityEngine.Random.Range(0, blueBlobPrefab.Length);

                //_blueBlobSelectedPrefabIndex[i] = 0;


                /* Create lineup TODO*/
                var blobT1 = Instantiate(blueBlobPrefab[_blueBlobSelectedPrefabIndex[i]], _blueStartPos[i], Quaternion.identity);
                blobT1.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                TeamOneBlobs.Add(blobT1.GetComponent<Combatant>());
                Combatant blobT1Script = blobT1.GetComponent<Combatant>();
                blobT1.transform.localScale = blobT1Script.LineupScale;
                blobT1Script.Team = 1;
                blobT1Script.ID = i + 1;
                ((Text)stats.ElementAt(i)).text = string.Format(BlobStatsFormat, i + 1, blobT1Script.GetHealth(), blobT1Script.GetAttack(), blobT1Script.GetDefense());


                _redBlobSelectedPrefabIndex[i] = UnityEngine.Random.Range(0, blueBlobPrefab.Length);
                //_redBlobSelectedPrefabIndex[i] = 0;
                var blobT2 = Instantiate(redBlobPrefab[_redBlobSelectedPrefabIndex[i]], _redStartPos[i], Quaternion.identity);
                blobT2.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                TeamTwoBlobs.Add(blobT2.GetComponent<Combatant>());
                Combatant blobT2Script = blobT2.GetComponent<Combatant>();
                blobT2.transform.localScale = blobT2Script.LineupScale;
                blobT2Script.ID = i + 1 + 3;
                blobT2Script.Team = 2;
                ((Text)stats.ElementAt(i + 3)).text = string.Format(BlobStatsFormat, i + 3 + 1, blobT2Script.GetHealth(), blobT2Script.GetAttack(), blobT2Script.GetDefense());


            }
        }

        private void MoveTeams()
        {

            for (int i = 0; i < 3; i++)
            {
                /* move from lineup to arena, TODO */
                TeamOneBlobs[i].transform.position = new Vector3((int)_selectedArena.TeamOnePos[i].x, (int)_selectedArena.TeamOnePos[i].y, (int)_selectedArena.TeamOnePos[i].z);

                TeamOneBlobs[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                TeamOneBlobs[i].SetTarget(TeamOneBlobs, TeamTwoBlobs, 1);
                TeamOneBlobs[i].transform.localScale = TeamOneBlobs[i].GetComponent<Combatant>().GameScale;
                //TeamOneBlobs[i].GetComponentInChildren<Canvas>().enabled = true;

                TeamTwoBlobs[i].transform.position = new Vector3((int)_selectedArena.TeamTwoPos[i].x, (int)_selectedArena.TeamTwoPos[i].y, (int)_selectedArena.TeamTwoPos[i].z);
                TeamTwoBlobs[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                TeamTwoBlobs[i].SetTarget(TeamOneBlobs, TeamTwoBlobs, 2);
                TeamTwoBlobs[i].transform.localScale = TeamTwoBlobs[i].GetComponent<Combatant>().GameScale;
                //TeamTwoBlobs[i].transform.localScale = new Vector2(_battleScale, _battleScale);
                //TeamTwoBlobs[i].GetComponentInChildren<Canvas>().enabled = true;
            }
        }

       

        private void StartCountdown()
        {
            _vsTimer += VsScreenTime;

            //Debug.Log("Start: "+_vsTimer);
        }

        private void UpdateBalancesOnRoundStart()
        {
            /* var bets = BettingService.Instance.GetTeamOneBets();
             bets.AddRange(BettingService.Instance.GetTeamTwoBets());
             foreach (var item in bets)
             {
                 Debug.Log(DataService.Instance.GetBalance(item.ViewerName));
                 Debug.Log("Balance subtracted for bet: " + item.ViewerName + "," + item.Amount);
                 DataService.Instance.UpdateBalance(item.ViewerName, item.Amount * -1);
                 Debug.Log(DataService.Instance.GetBalance(item.ViewerName));
             }*/
        }

        private void UpdateBalancesOnRoundEnd(int team)
        {
            Debug.Log(team);

            BettingService.Instance.PayoutBets(team);
        }

        private void SetupArenas()
        {
            _arenas = new List<ArenaData>();
            //ARENA POSITIONS
            Vector3[] testArenaOnePos = { new Vector3(-16, 5, 15), new Vector3(0, 5, 15), new Vector3(16, 5, 15), };
            Vector3[] testArenaTwoPos = { new Vector3(-16, 5, -10), new Vector3(0, 5, -10), new Vector3(16, 5, -10), };
            Vector3 testCameraPos = new Vector3(-34.9f, 30.2f, 0);
            Vector3 testCameraRot = new Vector3(37.548f, 90f, 0);


            _debugArena = new ArenaData("TestArenaTwo", testArenaOnePos, testArenaTwoPos, testCameraPos, testCameraRot, BackgroudMusic[0], "Debug Arena");

            //Shipwreck Island ARENA POSITIONS
            Vector3[] shipwreckIslandOnePos = { new Vector3(-235, 5, 87), new Vector3(-235, 5, 102), new Vector3(-235, 5, 117), };
            Vector3[] shipwreckIslandTwoPos = { new Vector3(-190, 5, 87), new Vector3(-190, 5, 102), new Vector3(-190, 5, 117), };
            Vector3 shipwrechIslandCameraPos = new Vector3(-216, 23, 25);
            Vector3 shipwrechIslandCameraRot = new Vector3(10, 0, 0);


            _arenas.Add(new ArenaData("PreBuiltPirates", shipwreckIslandOnePos, shipwreckIslandTwoPos, shipwrechIslandCameraPos, shipwrechIslandCameraRot, BackgroudMusic[0], "Shipwreck Island"));


        }
        #endregion

        #region Coroutines
        IEnumerator DelayBytTimeCoroutine(float time)
        {
            //Print the time of when the function is first called.
            Debug.Log("Started Coroutine at timestamp : " + Time.time);

            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(time);


            //After we have waited 5 seconds print the time again.
            Debug.Log("Finished Coroutine at timestamp : " + Time.time);
            EndBattleTwoEletricbogaloo();
        }

        IEnumerator DelayOneFrame()
        {

            //returning 0 will make it wait 1 frame
            yield return 0;

            //code goes here
            StartVsScreen();
        }


        public IEnumerator StartFade( float duration, float targetVolume)
        {
            var audio = GetComponent<AudioSource>();
            float currentTime = 0;
            float start = audio.volume;

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                audio.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
                yield return null;
            }
            yield break;
        }


        #endregion
    }
}

