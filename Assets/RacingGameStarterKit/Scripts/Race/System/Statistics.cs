using NWH.VehiclePhysics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Statistics.cs keeps track of the racer's rank, name, vehicle name, lap, race times, race state, saving best times, wrong way detecion etc.

namespace RGSK
{
    public class Statistics : MonoBehaviour
    {
        [System.Serializable]
        public class RacerDetails
        {
            public string racerName;
            public string vehicleName;
        }

        public RacerDetails racerDetails;

        //Int
        public int rank;//current rank
        public int lap; //current lap
        public int checkpoint; //current checkpoint(Checkpoint Race)

        //Strings
        public string currentLapTime; //current lap time string displayed by RaceUI.cs
        public string prevLapTime; //Previous lap time string displayed by RaceUI.cs
        public string totalRaceTime; //Total lap time string displayed by RaceUI.cs
        public string bestLapTime; //Best lap time string for current session;
        public Transform lastPassedNode; //last node to passed - used when respawning.
        //Floats
        [HideInInspector] public float lapTimeCounter; // keeps track of our current Lap time counter
        private float totalTimeCounter; //keeps track of our total race time
        private float prevLapCounter;
        private float bestLapCounter; //keeps track of the current session's best lap time
        private float dotProduct; //used for wrong way detection
        private float registerDistance = 5.0f; //distance to register a passed node
        private float reviveTimer;
        private float wrongwayTimer; //delay timer

        //Hidden Vars
        //[HideInInspector]

        [HideInInspector] 
        public Transform target; //progress tracker target
        [HideInInspector]
        public int currentNodeNumber = 0; //next node index in the "path" list
        [HideInInspector]
        public List<Transform> path = new List<Transform>();
        [HideInInspector]
        public List<bool> passednodes = new List<bool>();
        [HideInInspector]
        public List<Transform> checkpoints = new List<Transform>();
        [HideInInspector]
        public List<bool> passedcheckpoints = new List<bool>();
        [HideInInspector]
        public bool finishedRace;
        [HideInInspector]
        public bool knockedOut;
        [HideInInspector]
        public bool goingWrongway;
        [HideInInspector]
        public bool passedAllNodes;
        [HideInInspector]
        public bool infiniteLaps;
        [HideInInspector]
        public float speedRecord;//speed trap top speed

		[HideInInspector]
		public PathCreator.RaceMode raceMode;

        void Start()
        {
            if (!RaceManager.instance) { enabled = false; return; }

            FindPath();

            FindCheckpoints();

            Initialize();
        }

        void Initialize()
        {
            lap = 1;

            //Set the start timer for a checkpoint race
            if (RaceManager.instance._raceType == RaceManager.RaceType.Checkpoints)
            {
                lapTimeCounter = RaceManager.instance.initialCheckpointTime;
            }

            //Set the elimination timer for a eimination race
            if (RaceManager.instance._raceType == RaceManager.RaceType.Elimination)
            {
                lapTimeCounter = RaceManager.instance.eliminationTime;
            }

            //Set the time limit timer for a drift race
            if (RaceManager.instance._raceType == RaceManager.RaceType.Drift && RaceManager.instance.timeLimit)
            {
                lapTimeCounter = RaceManager.instance.driftTimeLimit;
            }

            infiniteLaps = RaceManager.instance._raceType == RaceManager.RaceType.TimeTrial;

			raceMode = RaceManager.instance.pathContainer.GetComponent<WaypointCircuit>().RaceMode;
        }


        void FindPath()
        {
            if (!RaceManager.instance.pathContainer)
                return;

            Transform[] nodes = RaceManager.instance.pathContainer.GetComponentsInChildren<Transform>();

            foreach (Transform p in nodes)
            {
                if (p != RaceManager.instance.pathContainer)
                {
                    path.Add(p);
                }
            }

			//Debug.Log(nodes.Length.ToString());

            passednodes = new List<bool>(new bool[path.Count]);

            lastPassedNode = path[0];
        }


        void FindCheckpoints()
        {
            if (!RaceManager.instance.checkpointContainer)
                return;

            Checkpoint[] _checkpoint = RaceManager.instance.checkpointContainer.GetComponentsInChildren<Checkpoint>();

            foreach (Checkpoint c in _checkpoint)
            {
                //Find SpeedTrap Checkpoints
                if (RaceManager.instance._raceType == RaceManager.RaceType.SpeedTrap)
                {
                    if (c.checkpointType == Checkpoint.CheckpointType.Speedtrap)
                    {
                        checkpoints.Add(c.transform);
                    }
                }

                //Find Time Checkpoints
                if (RaceManager.instance._raceType == RaceManager.RaceType.Checkpoints)
                {
                    if (c.checkpointType == Checkpoint.CheckpointType.TimeCheckpoint)
                    {
                        checkpoints.Add(c.transform);
                    }
                }
            }

            passedcheckpoints = new List<bool>(new bool[checkpoints.Count]);
        }


        void Update()
        {
            GetPath();

            CalculateRaceTimes();

            CheckForWrongway();

            CheckForRespawn();
        }


        void GetPath()
        {
			int			n			= currentNodeNumber;
			Transform	node		= path[n] as Transform;
			Vector3		nodeVector	= target.InverseTransformPoint(node.position);

			switch (raceMode)
			{
				case PathCreator.RaceMode.Circuit:
				{
					if (nodeVector.magnitude <= registerDistance)
					{
						currentNodeNumber++;

						passednodes[n] = true;

						if (n != 0)
							lastPassedNode = path[n - 1];
						else
							lastPassedNode = path[path.Count - 1];
					}

					foreach (bool pass in passednodes)
					{
						if (pass == true)
							passedAllNodes = true;
						else
							passedAllNodes = false;
					}

					if (currentNodeNumber >= path.Count)
					{
						currentNodeNumber = 0;
					}

					break;
				}
				case PathCreator.RaceMode.OneWay:
				{
					if (nodeVector.magnitude <= registerDistance)
					{
						currentNodeNumber++;

						passednodes[n] = true;

						if (n != 0)
							lastPassedNode = path[n];
						else
							lastPassedNode = path[0];
					}

					foreach (bool pass in passednodes)
					{
						if (pass == true)
							passedAllNodes = true;
						else
							passedAllNodes = false;
					}

					if (currentNodeNumber >= path.Count)
						currentNodeNumber = path.Count - 1;

					break;
				}
			}
        }


        // Race time calculations
        void CalculateRaceTimes()
        {
            if (RaceManager.instance.raceStarted && !knockedOut && !finishedRace)
            {
                if (RaceManager.instance.timerType == RaceManager.TimerType.CountUp)
                {
                    lapTimeCounter += Time.deltaTime;
                }

                if (RaceManager.instance.timerType == RaceManager.TimerType.CountDown)
                {
                    lapTimeCounter -= Time.deltaTime;

                    if (lapTimeCounter <= 0)
                    {
                        lapTimeCounter = 0;

                        if (RaceManager.instance._raceType == RaceManager.RaceType.Checkpoints)
                        {
                            knockedOut = true;

                            RaceManager.instance.KnockoutRacer(this);
                        }

                        if (RaceManager.instance._raceType == RaceManager.RaceType.Drift)
                        {
                            if (!knockedOut && !finishedRace)
                                FinishRace();
                        }
                    }
                }

                totalTimeCounter += Time.deltaTime;
            }

            //Format the time strings
            currentLapTime = RaceManager.instance.FormatTime(lapTimeCounter);

            totalRaceTime = RaceManager.instance.FormatTime(totalTimeCounter);
        }


        public void NewLap()
        {
            if (finishedRace || knockedOut) return;

            prevLapTime		= currentLapTime;
            prevLapCounter	= lapTimeCounter;

            CheckForBestTime();

            //Reset passed nodes
            for (int i = 0; i < passednodes.Count; i++)
            {
                passednodes[i] = false;
            }

            //Reset passed checkpoints
            for (int i = 0; i < passedcheckpoints.Count; i++)
            {
                passedcheckpoints[i] = false;
            }

            //Lap increment logic
            lap++;

            if (!infiniteLaps)
            {
                //Show final lap indication on the final lap
                if (lap == RaceManager.instance.totalLaps && RaceManager.instance.showRaceInfoMessages && gameObject.tag == "Player")
                    RaceUI.instance.ShowRaceInfo("Final Lap!", 2.0f, Color.white);


                if (lap > RaceManager.instance.totalLaps)
                {
                    if (!knockedOut && !finishedRace)
                        FinishRace();
                }
            }


            //Reset the lap timer based on the Timer Type
            if (RaceManager.instance.timerType != RaceManager.TimerType.CountDown)
            {
                lapTimeCounter = 0.0f;
            }

            //Check for knockout
            if (RaceManager.instance._raceType == RaceManager.RaceType.LapKnockout)
            {
                if (this.rank == RankManager.instance.currentRacers - 1)
                    RaceManager.instance.KnockoutRacer(RaceManager.instance.GetLastPlace());
            }
        }

        public void FinishRace()
        {
            if (finishedRace) return;

            finishedRace = true;

            //Player finish
            if (gameObject.tag == "Player")
            {
                RaceManager.instance.EndRace(rank);
            }

            //Continue after finishing
            if (RaceManager.instance.continueAfterFinish)
            {
                AIMode();
            }
            else
            {
				GetComponent<VehicleController>().Active = false;
				
				foreach (Wheel wheel in GetComponent<VehicleController>().Wheels)
					wheel.SetBrakeIntensity(0.5f);

				StartCoroutine
				(
					RaceUI.instance.arrow.GetComponent<HighlightText>().HightlightTextLerp()
				);
				//GetComponent<VehicleController>().input.Vertical = 0;
				//GetComponent<VehicleController>().input.Horizontal = 0;
				//GetComponent<Rigidbody>().velocity = Vector3.zero;

				//RaceManager.instance.DisableRacerInput(gameObject);
				//gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
				//gameObject.GetComponent<Rigidbody>().isKinematic = true;
			}
        }


        // Switches a player car to an AI controlled car
        public void AIMode()
        {
            if (GetComponent<PlayerControl>())
            {
                GetComponent<PlayerControl>().enabled = false;

                if (GetComponent<OpponentControl>())
                {
                    GetComponent<OpponentControl>().enabled = true;
                }
                else
                {
                    gameObject.AddComponent<OpponentControl>();
                }
            }
        }


        // Switches a AI car to a human controlled car
        public void PlayerMode()
        {
            if (GetComponent<OpponentControl>())
            {

                GetComponent<OpponentControl>().enabled = false;

                if (GetComponent<PlayerControl>())
                {
                    GetComponent<PlayerControl>().enabled = true;
                }
                else
                {
                    gameObject.AddComponent<PlayerControl>();
                }
            }
        }


        public void RegisterCheckpoint(Checkpoint.CheckpointType type, float timeAdd)
        {
            if (finishedRace || knockedOut) return;

            switch (type)
            {

                case Checkpoint.CheckpointType.Speedtrap:
                    if (RaceManager.instance._raceType != RaceManager.RaceType.SpeedTrap)
                        return;

                    //add to the racers total speed
                    float speed = 0;

                    if (GetComponent<Car_Controller>())
                        speed = GetComponent<Car_Controller>().currentSpeed;

                    if (GetComponent<Motorbike_Controller>())
                        speed = GetComponent<Motorbike_Controller>().currentSpeed;

                    speedRecord += speed;

                    //play a sound and show info
                    if (gameObject.tag == "Player")
                    {
                        try { SoundManager.instance.PlayDefaultSound(SoundManager.instance.defaultSounds.speedTrapSound); } catch { }

                        if (RaceManager.instance.showRaceInfoMessages)
                            RaceUI.instance.ShowRaceInfo("+ " + speed + " mph", 1.0f, Color.white);
                    }

                    break;

                case Checkpoint.CheckpointType.TimeCheckpoint:
                    //add our chekpoint
                    if(checkpoint < checkpoints.Count)
                        checkpoint++;

                    //add to the timer
                    lapTimeCounter += timeAdd;

                    //play a sound and show info
                    if (gameObject.tag == "Player")
                    {
                        try { SoundManager.instance.PlayDefaultSound(SoundManager.instance.defaultSounds.checkpointSound); } catch { }

                        if (RaceManager.instance.showRaceInfoMessages)
                            RaceUI.instance.ShowRaceInfo("+ " + RaceManager.instance.FormatTime(timeAdd), 1.0f, Color.white);
                    }
                    break;
            }
        }

        void GhostVehicleLogic(bool firstLap, bool beatLastLap)
        {
            if (!RaceManager.instance.enableGhostVehicle || !gameObject.GetComponent<GhostVehicle>()) return;

            //Always create a ghost and cache values after the first lap
            if (firstLap)
            {
                GetComponent<GhostVehicle>().CacheValues();
                RaceManager.instance.CreateGhostVehicle(gameObject);
            }

            //Create a ghost & cache the values if we beat the last ghost
            if (beatLastLap)
            {
                GetComponent<GhostVehicle>().CacheValues();
                RaceManager.instance.CreateGhostVehicle(gameObject);
            }
            else
            {
                //Use the cached values if we dont beat the last lap
                if (!firstLap)
                {
                    GetComponent<GhostVehicle>().UseCachedValues();
                    RaceManager.instance.CreateGhostVehicle(gameObject);
                }
            }

            //Reset the recorded values
            GetComponent<GhostVehicle>().ClearValues();
        }

        void CheckForBestTime()
        {
            //Best Lap Time
            if (bestLapCounter == 0)
            {
                bestLapCounter = lapTimeCounter;
                bestLapTime = RaceManager.instance.FormatTime(bestLapCounter);
                GhostVehicleLogic(true, false);
            }

            else if (prevLapCounter < bestLapCounter)
            {
                bestLapCounter = prevLapCounter;
                bestLapTime = RaceManager.instance.FormatTime(bestLapCounter);
                GhostVehicleLogic(false, true);
            }

            else if (prevLapCounter > bestLapCounter)
            {
                GhostVehicleLogic(false, false);
            }

            //Save Best Track Lap Time
            if (gameObject.tag == "Player")
            {
                //Set new best
                if (!PlayerPrefs.HasKey("BestTimeFloat" + SceneManager.GetActiveScene().name))
                {
                    PlayerPrefs.SetString("BestTime" + SceneManager.GetActiveScene().name, RaceManager.instance.FormatTime(lapTimeCounter));

                    PlayerPrefs.SetFloat("BestTimeFloat" + SceneManager.GetActiveScene().name, Mathf.Abs(lapTimeCounter));

                    if (RaceManager.instance.showRaceInfoMessages)
                    {
                        RaceUI.instance.ShowRaceInfo("New best time!", 2.0f, Color.white);
                    }
                }

                //Beat our best
                //본래 맵마다 정해진 모드가 없어서 그냥 랩타임이 작은게 베스트타임으로 갔으나 현재 락버기 시스템은 TimeLimit(CheckPoint)만 사용하는 맵이 있어 가장 남은시간이 많은게 베스트타임
                switch (RaceManager.instance._raceType)
                {
                    case RaceManager.RaceType.Checkpoints:
                    case RaceManager.RaceType.Elimination: // Checkpoint와 같이 TimerType.CountDown 설정 사용
                        if (PlayerPrefs.GetFloat("BestTimeFloat" + SceneManager.GetActiveScene().name) < lapTimeCounter)
                        {
                            PlayerPrefs.SetString("BestTime" + SceneManager.GetActiveScene().name, RaceManager.instance.FormatTime(lapTimeCounter));

                            PlayerPrefs.SetFloat("BestTimeFloat" + SceneManager.GetActiveScene().name, lapTimeCounter);

                            if (RaceManager.instance.showRaceInfoMessages)
                            {
                                RaceUI.instance.ShowRaceInfo("New best time!", 2.0f, Color.white);
                            }
                        }
                        break;
                    default:
                        if (PlayerPrefs.GetFloat("BestTimeFloat" + SceneManager.GetActiveScene().name) > lapTimeCounter)
                        {
                            PlayerPrefs.SetString("BestTime" + SceneManager.GetActiveScene().name, RaceManager.instance.FormatTime(lapTimeCounter));

                            PlayerPrefs.SetFloat("BestTimeFloat" + SceneManager.GetActiveScene().name, lapTimeCounter);

                            if (RaceManager.instance.showRaceInfoMessages)
                            {
                                RaceUI.instance.ShowRaceInfo("New best time!", 2.0f, Color.white);
                            }
                        }
                        break;
                }
            }
        }

        void CheckForWrongway()
        {
			//return;

            float nodeAngle = target.transform.eulerAngles.y;

            float transformAngle = transform.eulerAngles.y;

            float angleDifference = nodeAngle - transformAngle;

            //Set wrong way to true after a dealy of 1.0 seconds
            goingWrongway = (wrongwayTimer >= 1.0f);

			bool bWrongwayChecking = false;

			switch (raceMode)
			{
				case PathCreator.RaceMode.Circuit:
				{
					bWrongwayChecking = Mathf.Abs(angleDifference) <= 230f && Mathf.Abs(angleDifference) >= 120;
					break;
				}
				case PathCreator.RaceMode.OneWay:
				{
					bWrongwayChecking = Mathf.Abs(angleDifference) <= 270.0f && Mathf.Abs(angleDifference) >= 90.0f;
					break;
				}
			}

            if (bWrongwayChecking)
            {
                //Add/reset the timer
                if (GetComponent<Rigidbody>().velocity.magnitude >= 5.0f)
                {
                    wrongwayTimer += Time.deltaTime;
                }
                else
                {
                    wrongwayTimer = 0.0f;
                }
            }
            else
            {
                wrongwayTimer = 0.0f;
            }
        }

        void CheckForRespawn()
        {
            if (finishedRace && knockedOut)
                return;

            //incase the car flips over or going wrong way then respawn
            if (transform.localEulerAngles.z > 80 && transform.localEulerAngles.z < 280 || RaceManager.instance.forceWrongwayRespawn && goingWrongway)
            {
                reviveTimer += Time.deltaTime;
            }
            else
            {
                reviveTimer = 0.0f;
            }

            if (reviveTimer >= 5.0f)
            {
                RaceManager.instance.RespawnRacer(transform, lastPassedNode, 0.1f);

                reviveTimer = 0.0f;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            //Finish line
            if (other.tag == "FinishLine" || other.tag == "Finish")
            {
				switch (raceMode)
				{
					case PathCreator.RaceMode.Circuit:
					{
						if (passedAllNodes)
							NewLap();

						break;
					}
					case PathCreator.RaceMode.OneWay:
					{
						if (passedAllNodes)
						{
							RaceManager.instance.continueAfterFinish = false;

							NewLap();
						}
						break;
					}
				}
            }

            //Checkpoint
            if (other.GetComponent<Checkpoint>())
            {
                for (int i = 0; i < checkpoints.Count; i++)
                {
                    if (checkpoints[i] == other.transform && !passedcheckpoints[i])
                    {
                        passedcheckpoints[i] = true;

                        RegisterCheckpoint(checkpoints[i].GetComponent<Checkpoint>().checkpointType, checkpoints[i].GetComponent<Checkpoint>().timeToAdd);
                    }
                }
            }
        }
    }
}
