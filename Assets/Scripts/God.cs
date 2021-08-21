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

namespace Assets
{
    public class God : MonoBehaviour
    {
        const string BlobStatsFormat = "Blob {0}\nHp: {1}\nAttack: {2}\nDefense: {3}";

        [SerializeField]
        private List<Combatant> TeamOneBlobs;
        [SerializeField]
        private List<Combatant> TeamTwoBlobs;
        [SerializeField]
        GameObject blueBlobPrefab;
        [SerializeField]
        GameObject redBlobPrefab;


        private Text countDown;
        private float _vsTimer = 0.0f;
        private const int VsScreenTime = 11; //Add an extra 1 as the timer imediately starts at 10.9 which rounds down to 10, instead of 9.9 => 9
        private int _count = 0;
        private const float _vsScale = .25f;
        private const float _battleScale = .38f;

        //private IBrain[] _teamOneBrains = { new GuardianBrain(), new GuardianBrain(), new HealerBrain(), };
        //private IClass[] _teamOneClasses = { new GuardianClass(), new GuardianClass(), new HealerClass(), };
        /*private IBrain[] _teamOneBrains = { new WarriorBrain() , new WarriorBrain(), new WarriorBrain(), };
        private IClass[] _teamOneClasses = { new WarriorClass() , new WarriorClass(), new WarriorClass(), };
        private IBrain[] _teamTwoBrains = { new HealerBrain(), new GuardianBrain(), new GuardianBrain(), };
        private IClass[] _teamTwoClasses = { new HealerClass(), new GuardianClass(), new GuardianClass(), };*/

        //Selected Positions

        private Vector3[] _teamOnePos;
        private Vector3[] _teamTwoPos;
        private Vector3 _selectedCameraPos;
        private Vector3 _selectedCameraRot;

        //ARENA POSITIONS
        private Vector3[] _testArenaOnePos = { new Vector3(-16, 5, 15) , new Vector3(0, 5, 15), new Vector3(16, 5, 15), };
        private Vector3[] _testArenaTwoPos = { new Vector3(-16, 5, -10), new Vector3(0, 5, -10), new Vector3(16, 5, -10), };
        private Vector3 _testCameraPos = new Vector3(-34.9f, 30.2f, 0);
        private Vector3 _testCameraRot = new Vector3(37.548f, 90f, 0);

        //Shipwreck Island ARENA POSITIONS
        private Vector3[] _shipwreckIslandOnePos = { new Vector3(-235, 5, 87), new Vector3(-235, 5, 102), new Vector3(-235, 5, 117), };
        private Vector3[] _shipwreckIslandTwoPos = { new Vector3(-190, 5, 87), new Vector3(-190, 5, 102), new Vector3(-190, 5, 117), };
        private Vector3 _shipwrechIslandCameraPos = new Vector3(-216, 23, 25);
        private Vector3 _shipwrechIslandCameraRot = new Vector3(10, 0, 0);

        //LINEUP POSITIONS
        private Vector2[] _blueStartPos = { new Vector2(-3.67f, 2.76f), new Vector2(-3.67f, -.1f), new Vector2(-3.67f, -3.5f), };
        private Vector2[] _redStartPos = { new Vector2(3.55f, 2.91f) , new Vector2(3.55f, -.1f), new Vector2(3.55f, -3.5f), };

        private TwitchChatBot tcb;


        public God()
        {
            //instance = this;
        }

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

        public bool IsBattling { get; private set; } = false;
        public Vector3 SelectedCameraPos { get => _selectedCameraPos; set => _selectedCameraPos = value; }
        public Vector3 SelectedCameraRot { get => _selectedCameraRot; set => _selectedCameraRot = value; }

        // Start is called before the first frame update
        void Start()
        {
            instance = this;
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

        private void StartDatabaseManager()
        {
           
            if (!DataService.Instance.CheckDatabase())
            {
                if (!DataService.Instance.CreateDatabase())
                {
                    Debug.Log("Failed to create database ");
                    return;
                }
            }

            //DataService.Instance.UpdateBalance("kalloc656", 500);
            Debug.Log(DataService.Instance.GetBalance("kalloc656"));

        }

        private void StartTwitchBot()
        {

            tcb = new TwitchChatBot(
            server: "irc.chat.twitch.tv",
            port: 6667,
            nick: "BlobArenaCoordinator",
            channel: "kalloc656"
            );

            //tcb.Start();

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey("escape"))
            {

                if (tcb != null)
                {
                    tcb.OnEnd();
                }
                Application.Quit();
            }

            if (!IsBattling)
            {
                _vsTimer -= Time.deltaTime;
                if (countDown != null)
                {
                    countDown.text = Math.Floor(_vsTimer).ToString();
                    if (_vsTimer <= 0)
                    {
                        StartBattle();
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
            if (tcb != null)
            {
                tcb.OnEnd();
            }
        }

        public void StartVsScreen()
        {
            //Debug.Log("start vs");

            var temp = GameObject.FindWithTag("CountDown");

            //Debug.Log(temp != null);
            if (temp != null)
            {
                //Debug.Log("found text");
                countDown = temp.GetComponent<Text>();
            }



            CreateLineup();
            StartCountdown();
            BettingService.Instance.StartNewRound();
            IsBattling = false;
        }

        public void StartBattle()
        {
            var scene = ChooseArena();
            SceneManager.LoadScene(scene);

            Task task = new Task (() => UpdateBalancesOnRoundStart());
            task.Start();
            MoveTeams();
            IsBattling = true;

            
        }

        IEnumerator ExampleCoroutine(float time)
        {
            //Print the time of when the function is first called.
            Debug.Log("Started Coroutine at timestamp : " + Time.time);

            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(time);

           
            //After we have waited 5 seconds print the time again.
            Debug.Log("Finished Coroutine at timestamp : " + Time.time);
            EndBattleTwoEletricbogaloo();
        }

        

        public void EndBattle()
        {
            IsBattling = false;
            _vsTimer = 0;

            //add delay????
            StartCoroutine(ExampleCoroutine(3));

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
                if (blobItem.target.GetComponent<Combatant>().ID == blob.ID)
                {
                    blobItem.SetTarget(TeamOneBlobs, TeamTwoBlobs, 1);
                }
            }
            foreach (var blobItem in TeamTwoBlobs)
            {
                if (blobItem.target.GetComponent<Combatant>().ID == blob.ID)
                {
                    blobItem.SetTarget(TeamOneBlobs, TeamTwoBlobs, 2);
                }
            }

            blob.OnDestroy();
            GameObject.Destroy(blob.gameObject);
        }
        IEnumerator DelayOneFrame()
        {

            //returning 0 will make it wait 1 frame
            yield return 0;

            //code goes here
            StartVsScreen();
        }

        private string ChooseArena()
        {
            //var map = UnityEngine.Random.Range(1, 3);
            var map = 2;
            switch (map)
            {
                case 1:
                    _teamOnePos = _testArenaOnePos;
                    _teamTwoPos = _testArenaTwoPos;
                    SelectedCameraPos = _testCameraPos;
                    SelectedCameraRot = _testCameraRot;
                    return "TestArenaTwo";
                case 2:
                    _teamOnePos = _shipwreckIslandOnePos;
                    _teamTwoPos = _shipwreckIslandTwoPos;
                    SelectedCameraPos = _shipwrechIslandCameraPos;
                    SelectedCameraRot = _shipwrechIslandCameraRot;
                    return "PreBuiltPirates";
                case 3:
                //272.22, 20, -253.15 a good spot
                default:
                    break;
            }

            return "TestArenaTwo";

        }

        private void MoveTeams()
        {

            for (int i = 0; i < 3; i++)
            {
                /* move from lineup to arena, TODO */
                TeamOneBlobs[i].transform.position = new Vector3(_teamOnePos[i].x, (int)_teamOnePos[i].y, (int)_teamOnePos[i].z);

                TeamOneBlobs[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                TeamOneBlobs[i].SetTarget(TeamOneBlobs, TeamTwoBlobs, 1);
                //TeamOneBlobs[i].transform.localScale = new Vector2(_battleScale, _battleScale);
                //TeamOneBlobs[i].GetComponentInChildren<Canvas>().enabled = true;

                TeamTwoBlobs[i].transform.position = new Vector3((int)_teamTwoPos[i].x, (int)_teamTwoPos[i].y, (int)_teamTwoPos[i].z);
                TeamTwoBlobs[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                TeamTwoBlobs[i].SetTarget(TeamOneBlobs, TeamTwoBlobs, 2);
                //TeamTwoBlobs[i].transform.localScale = new Vector2(_battleScale, _battleScale);
                //TeamTwoBlobs[i].GetComponentInChildren<Canvas>().enabled = true;
            }
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
                /* Create lineup TODO*/
                var blobT1 = Instantiate(blueBlobPrefab, _blueStartPos[i], Quaternion.identity);
                blobT1.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                //blobT1.GetComponent<SpriteRenderer>().color = Color.blue;
                //blobT1.GetComponent<Combatant>().SetClass(_teamOneClasses[i], _teamOneBrains[i], this);
                //blobT1.GetComponentInChildren<Canvas>().enabled = false;
                //blobT1.transform.localScale = new Vector2(_vsScale, _vsScale);
                TeamOneBlobs.Add(blobT1.GetComponent<Combatant>());
                Combatant blobT1Script = blobT1.GetComponent<Combatant>();
                blobT1Script.ID = i+1;
                ((Text)stats.ElementAt(i)).text = string.Format(BlobStatsFormat, i + 1, blobT1Script.GetHealth(), blobT1Script.GetAttack(), blobT1Script.GetDefense());

                var blobT2 = Instantiate(redBlobPrefab, _redStartPos[i], Quaternion.identity);
                blobT2.GetComponent<Rigidbody>().constraints =  RigidbodyConstraints.FreezeAll;
                //blobT2.GetComponent<SpriteRenderer>().color = Color.red;
                //blobT2.GetComponent<BlobScript>().SetClass(_teamTwoClasses[i], _teamTwoBrains[i], this);
                //blobT2.GetComponentInChildren<Canvas>().enabled = false;
                //blobT2.transform.localScale = new Vector2(_vsScale, _vsScale);
                TeamTwoBlobs.Add(blobT2.GetComponent<Combatant>());
                Combatant blobT2Script = blobT2.GetComponent<Combatant>();
                blobT2Script.ID = i+1+3;
                ((Text)stats.ElementAt(i + 3)).text = string.Format(BlobStatsFormat, i + 3 + 1, blobT2Script.GetHealth(), blobT2Script.GetAttack(), blobT2Script.GetDefense());
                

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
    }
}

