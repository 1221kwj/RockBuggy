//Race_Manager.cs handles the race logic - countdown, spawning cars, asigning racer names, checking race status, formatting time strings and more important race functions.
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using NWH.VehiclePhysics;

namespace RGSK
{
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager instance;

		#region enum
		public enum RaceType { Circuit, LapKnockout, TimeTrial, SpeedTrap, Checkpoints, Elimination, Drift, FuelLimit }
		
		public enum RaceTime { Day = 0, Night }
		public enum WeatherType { Fine, Rain, Fog }

		public enum RaceState { StartingGrid, Racing, Paused, Complete, KnockedOut, Replay }

		/**
		 * 필요하지 않을 요소
		 */
		public enum PlayerSpawnPosition { Randomized, Selected }

		public enum TimerType { CountUp, CountDown }

		/**
		 * 필요하지 않을 요소
		 */
		public enum AISpawnType { Randomized, Order }
        public OpponentControl.AiDifficulty aiDifficulty = OpponentControl.AiDifficulty.Custom;

		public RaceType _raceType;
        public RaceState _raceState = RaceState.StartingGrid;
        public PlayerSpawnPosition _playerSpawnPosition;
		/**
		 * 필요하지 않을 요소
		 */
		public AISpawnType _aiSpawnType;

        public TimerType timerType = TimerType.CountUp;
        #endregion

        #region int
        public int totalLaps;
        public int totalRacers; //The total number of racers (player included)
        public int playerStartRank; //The rank you will start the race as
        public int countdownFrom;
        private int currentCountdownTime;
        #endregion

        #region float
        public float raceDistance; //Your race track's distance.
        public float countdownDelay = 3.0f;
        private float countdownTimer = 1.0f;
        public float initialCheckpointTime = 10.0f; //start time (Checkpoint race);
        public float driftTimeLimit = 60; //time limit (Drift race)
        public float eliminationTime = 30f; //start time (Elimination race);
        public float eliminationCounter; //timer for elimination
        public float ghostAlpha = 0.3f;
        public float goldDriftPoints = 10000;
        public float silverDriftPoints = 5000;
        public float bronzeDriftPoints = 1000;
        #endregion

        #region Transform
        public Transform pathContainer;
        public Transform spawnpointContainer;
        public Transform checkpointContainer;
        public Transform timeTrialStartPoint;
        #endregion

        #region GameObject
        public GameObject playerCar;
        public GameObject playerPointer, opponentPointer, racerName;
        public GameObject activeGhostCar;

		[HideInInspector] public GameObject currentPlayer;
        #endregion

        #region List
        public List<GameObject> opponentCars = new List<GameObject>();
        public List<Transform> spawnpoints = new List<Transform>();
        public List<RaceRewards> raceRewards = new List<RaceRewards>();
        public List<string> opponentNamesList = new List<string>();
        public List<Statistics> eliminationList = new List<Statistics>();
        #endregion

        public TextAsset opponentNames;
        public StringReader nameReader;
        public string playerName = "You";
        public Shader ghostShader;
        public Material ghostMaterial;

        #region bool
        private bool startCountdown;
        public bool continueAfterFinish = false; //Should the racers keep driving after finish.
        public bool showRacerNames = true; //Should names appear above player cars
        public bool showRacerPointers = true; //Should minimap pointers appear above all racers
        public bool showRaceInfoMessages = true;//Show final lap indication , new best lap, speed trap & racer knockout information texts
        public bool forceWrongwayRespawn; //should the player get respawned if going the wrong way
        public bool raceStarted; //has the race began
        public bool raceCompleted; //has the race began
        public bool loadRacePreferences; //Load menu prefrences?
        public bool allowDuplicateRacers;//allow duplicate AI
        public bool assignAiRacerNames = true;
        public bool assignPlayerName = true;
        public bool enableGhostVehicle = true;
        public bool useGhostMaterial = false;
        public bool enableReplay = true;
        public bool autoStartReplay; //automatically start the replay after finishing the race
        public bool showStartingGrid;
        public bool timeTrialAutoDrive = true;
        public bool penalties = true;
        public bool timeLimit;
		public bool bRespawn = false;
        #endregion

        #region Default_FuelValues
        public bool useFuel_Default = true;
        public float capacity_Default = 50f; //50
        public float amount_Default = 50f; //50
        public float efficiency_Default = 0.45f; //0.45
        #endregion

        #region FuelLimitMode_FuelValues
        public bool useFuel_Limit = true;
        public float capacity_Limit = 1.6f; //50
        public float amount_Limit = 1.6f; //50
        public float efficiency_Limit = 0.001f; //0.45
		#endregion

		#region RaceTimeWeather
		public RaceTime raceTime;
		public Transform sun;
		public MeshRenderer low;
		public MeshRenderer high;
		#endregion

		void Awake()
		{
			//create an instance
			instance = this;

			//load race prefernces from an active data loader
			if (loadRacePreferences)
			{
				if (GameObject.FindObjectOfType(typeof(DataLoader)))
				{
					DataLoader dl = GameObject.FindObjectOfType(typeof(DataLoader)) as DataLoader;
					dl.LoadRacePreferences();
				}
				else
				{
#if UNITY_EDITOR
					Debug.LogWarning("Add a DataLoader component to your scene to load race preferences!");
#endif
				}
			}
		}

		void Start()
		{

			if (PlayerPrefs.HasKey("RaceTime"))
				raceTime = (RaceTime)PlayerPrefs.GetInt("RaceTime");

			RaceTimeSetting(raceTime);

			//Set appropriate racer & lap values according to race type.
			switch (_raceType)
			{
				case RaceType.Circuit:
					timerType = TimerType.CountUp;
					continueAfterFinish = true;
					showStartingGrid = false;

					break;

				/*case RaceType.Sprint:
                    totalLaps = 1;
                    continueAfterFinish = false;
                    timerType = TimerType.CountUp;
                    break;
                */

				case RaceType.LapKnockout:
					if (totalRacers < 2)
					{
						totalRacers = 2;
					}
					totalLaps = totalRacers - 1;
					timerType = TimerType.CountUp;
					break;

				case RaceType.TimeTrial:
					totalRacers = 1;
					enableReplay = false;
					showStartingGrid = false;
					timerType = TimerType.CountUp;
					if (SoundManager.instance.musicStart == SoundManager.MusicStart.BeforeCountdown)
						SoundManager.instance.musicStart = SoundManager.MusicStart.AfterCountdown;
					break;

				case RaceType.Checkpoints:
					if (totalRacers == 1) showStartingGrid = false;
					timerType = TimerType.CountDown;
					break;

				case RaceType.Elimination:
					if (totalRacers < 2)
					{
						totalRacers = 2;
					}
					eliminationCounter = eliminationTime;
					timerType = TimerType.CountDown;
					break;

				case RaceType.Drift:
					totalRacers = 1;
					showStartingGrid = false;
					timerType = (timeLimit) ? TimerType.CountDown : TimerType.CountUp;
					break;

				case RaceType.FuelLimit:
					totalRacers = 1;
					totalLaps = 1;
					//enableReplay = false;
					showStartingGrid = false;
					timerType = TimerType.CountUp;
					break;
			}

			ConfigureNodes();
			SpawnRacers();
		}

		private void RaceTimeSetting(RaceTime raceTime)
		{
			switch (raceTime)
			{
				case RaceTime.Day:
				{
					// Sky
					{
						sun.GetComponent<Light>().color = new Color
						(
							245.0f / 255.0f,
							245.0f / 255.0f,
							180.0f / 255.0f,
							1.0f
						);
						sun.GetComponent<Light>().intensity = 1.5f;

						RenderSettings.ambientIntensity = 1.5f;
						RenderSettings.skybox.SetFloat("_AtmosphereThickness", 1.0f);
						RenderSettings.skybox.SetColor
						(
							"_SkyTint",
							new Color(144.0f /255.0f, 144.0f / 255.0f, 144.0f / 255.0f, 1.0f)
						);
						RenderSettings.skybox.SetColor
						(
							"_GroundColor",
							new Color(99.0f / 255.0f, 89.0f / 255.0f, 85.0f / 255.0f, 1.0f)
						);
						RenderSettings.skybox.SetFloat("_Exposure", 1.3f);
					}

					// Fog Setting
					{
						RenderSettings.fog = true;
						RenderSettings.fogColor = new Color
						(
							1.0f, 1.0f, 210.0f / 255.0f, 1.0f
						);
						RenderSettings.fogMode = FogMode.ExponentialSquared;
						RenderSettings.fogDensity = 0.005f;
					}

					// Cloud Material
					if (low != null && high != null)
					{
						low.material.SetColor
						(
							"_CloudColor",
							new Color(1.0f, 1.0f, 1.0f, 130.0f / 255.0f)
						);
						high.material.SetColor
						(
							"_CloudColor",
							new Color(1.0f, 1.0f, 1.0f, 100.0f / 255.0f)
						);
					}

					
					break;
				}
				case RaceTime.Night:
				{
					// Sky
					{
						sun.GetComponent<Light>().color = new Color
						(
							245.0f / 255.0f,
							245.0f / 255.0f,
							180.0f / 255.0f,
							1.0f
						);
						sun.GetComponent<Light>().intensity = 0.1f;

						RenderSettings.ambientIntensity = 0.1f;
						RenderSettings.skybox.SetFloat("_AtmosphereThickness", 0.0f);
						RenderSettings.skybox.SetColor
						(
							"_SkyTint",
							new Color(0, 0, 0, 1.0f)
						);
						RenderSettings.skybox.SetColor
						(
							"_GroundColor",
							new Color(53.0f / 255.0f, 49.0f / 255.0f, 48.0f / 255.0f, 1.0f)
						);
						RenderSettings.skybox.SetFloat("_Exposure", 0.35f);
					}

					// Fog Setting
					{
						RenderSettings.fog = true;
						RenderSettings.fogColor = new Color
						(
							50.0f / 255.0f, 50.0f / 255.0f, 50.0f / 255.0f, 1.0f
						);
						RenderSettings.fogMode = FogMode.ExponentialSquared;
						RenderSettings.fogDensity = 0.01f;
					}

					// Cloud Material
					if (low != null && high != null)
					{
						low.material.SetColor
						(
							"_CloudColor",
							new Color
							(
								25.0f / 255.0f,
								25.0f / 255.0f,
								25.0f / 255.0f,
								150.0f / 255.0f
							)
						);
						high.material.SetColor
						( 
							"_CloudColor",
							new Color
							(
								50.0f / 255.0f,
								50.0f / 255.0f,
								50.0f / 255.0f,
								130.0f / 255.0f
							)
						);
					}
					break;
				}
			}


			LampMgr mgr = GameObject.FindObjectOfType<LampMgr>();

			if (mgr != null)
				mgr.Initialize();

			//MobileInputManager mim = GameObject.FindObjectOfType<MobileInputManager>();
			//mim.Initialize();
		}


        void SpawnRacers()
        {

            if (!playerCar)
            {
                Debug.LogError("Please add a player vehicle!");
                return;
            }


            //Find the children of the spawnpoint container and add them to the spawnpoints List.
            spawnpoints.Clear();

            Transform[] _sp = spawnpointContainer.GetComponentsInChildren<Transform>();

            foreach (Transform point in _sp)
            {
                if (point != spawnpointContainer)
                {
                    spawnpoints.Add(point);
                }
            }

            //Set appropriate values incase they are icnorrectly configured.
            totalRacers = SetValue(totalRacers, spawnpoints.Count);

            playerStartRank = SetValue(playerStartRank, totalRacers);

            totalLaps = SetValue(totalLaps, 1000);

            //Check for player spawn type
            if (_playerSpawnPosition == PlayerSpawnPosition.Randomized)
            {
                playerStartRank = Random.Range(1, totalRacers);
            }

            //Randomize spawn if total racers is greater than AI
            if (totalRacers - 1 > opponentCars.Count)
            {
                _aiSpawnType = AISpawnType.Randomized;

                allowDuplicateRacers = true;
            }

            //Spawn the racers
            for (int i = 0; i < totalRacers; i++)
            {
                if (spawnpoints[i] != spawnpoints[playerStartRank - 1] && opponentCars.Count > 0)
                {
					//Spawn the AI

					GameObject aiGO = null;

                    if (_aiSpawnType == AISpawnType.Randomized)
                    {
						if (allowDuplicateRacers)
                        {
							aiGO = Instantiate(opponentCars[Random.Range(0, opponentCars.Count)], spawnpoints[i].position, spawnpoints[i].rotation);
                        }
                        else
                        {
                            int spawnIndex = Random.Range(0, opponentCars.Count);

                            if (spawnIndex > opponentCars.Count) spawnIndex = opponentCars.Count - 1;

							aiGO = Instantiate(opponentCars[spawnIndex], spawnpoints[i].position, spawnpoints[i].rotation);

                            opponentCars.RemoveAt(spawnIndex);
                        }
					}
                    else if (_aiSpawnType == AISpawnType.Order)
                    {

                        int spawnIndex = 0;

                        if (spawnIndex > opponentCars.Count) spawnIndex = opponentCars.Count - 1;

						aiGO = Instantiate(opponentCars[spawnIndex], spawnpoints[i].position, spawnpoints[i].rotation);

						opponentCars.RemoveAt(spawnIndex);
                    }

					float randomNum = Random.Range(-1.0f, 1.0f);

					aiGO.GetComponent<AIMaterialList>().ChangeMaterial();
					aiGO.GetComponent<VehicleController>().fuel.useFuel = false;

					aiGO.GetComponent<VehicleController>().engine.maxRPM = 4500.0f + randomNum * 300.0f;
					aiGO.GetComponent<VehicleController>().engine.maxPower = 200.0f + randomNum * 10.0f;
                }
                else if (spawnpoints[i] == spawnpoints[playerStartRank - 1] && playerCar)
                {
                    //Spawn the player
                    //TimeTrial전용 시작위치는 의미없으므로 주석처리
                    Transform spawnPos = spawnpoints[i];//(_raceType != RaceType.TimeTrial) ? spawnpoints[i] : timeTrialStartPoint;

                    currentPlayer = (GameObject)Instantiate(playerCar, spawnPos.position, spawnPos.rotation);
					
					switch (_raceType)
                    {
						case RaceType.Circuit:
						{
							RaceUI.instance.FuelPanel.SetActive(false);
							currentPlayer.GetComponent<WaypointArrow>().show = true;
							currentPlayer.GetComponent<VehicleController>().fuel.useFuel = false;

							List<Wheel> ws = currentPlayer.GetComponent<VehicleController>().Wheels;

							foreach (Wheel w in ws)
								w.WheelController.singleRay = true;

							break;
						}
                        case RaceType.Drift:
						{
							if (!currentPlayer.GetComponent<DriftPointController>())
								currentPlayer.AddComponent<DriftPointController>();
							break;
						}
                        case RaceType.TimeTrial:
						{
							currentPlayer.AddComponent<TimeTrialConfig>();

							if (enableGhostVehicle)
								currentPlayer.AddComponent<GhostVehicle>();
							break;
						}

                        case RaceType.FuelLimit:
						{
							//if (currentPlayer.GetComponent<EngineOverhitController>() == null)
							//	currentPlayer.AddComponent<EngineOverhitController>();
							currentPlayer.GetComponent<VehicleController>().fuel.useFuel = true;
							List<Wheel> ws = currentPlayer.GetComponent<VehicleController>().Wheels;

							foreach (Wheel w in ws)
								w.WheelController.singleRay = false;

							currentPlayer.GetComponent<WaypointArrow>().show = true;

							if (!currentPlayer.GetComponent<FuelPointController>())
								currentPlayer.AddComponent<FuelPointController>();
							break;
						}

						case RaceType.Checkpoints:
						{
							currentPlayer.GetComponent<VehicleController>().fuel.useFuel = false;
							List<Wheel> ws = currentPlayer.GetComponent<VehicleController>().Wheels;

							foreach (Wheel w in ws)
								w.WheelController.singleRay = false;

							currentPlayer.GetComponent<WaypointArrow>().show = true;

							break;
						}
                    }
                }
            }

            //Set racer names, pointers and handle countdown after spawning the racers
            RankManager.instance.RefreshRacerCount();
            RaceUI.instance.RefreshInRaceStandings();
            SetRacerPreferences();

            //Start the countdown immediately if starting grid isn't shown
            if (!showStartingGrid)
            {
                StartCoroutine(Countdown(countdownDelay));
            }
            else
            {
                //Update cameras
                CameraManager.instance.ActivateStartingGridCamera();
            }
        }

        void SetRacerPreferences()
        {
            Statistics[] racers = GameObject.FindObjectsOfType(typeof(Statistics)) as Statistics[];

            //Load opponent names if they havent already been loaded
            if (opponentNamesList.Count <= 0)
            {
                LoadRacerNames();
            }

            for (int i = 0; i < racers.Length; i++)
            {
                racers[i].name = ReplaceString(racers[i].name, "(Clone)");
				VehicleController controller = racers[i].GetComponent<VehicleController>();

                if (racers[i].gameObject.tag == "Player")
                {
                    //Player Name & Player Minimap Pointer
                    if (assignPlayerName)
                    {
                        racers[i].racerDetails.racerName = playerName;
                    }

                    if (showRacerPointers && playerPointer)
                    {
                        GameObject m_pointer = (GameObject)Instantiate(playerPointer);

                        m_pointer.GetComponent<RacerPointer>().target = racers[i].transform;
                    }
                }
                else
                {
                    //AI Racer Names
                    if (assignAiRacerNames)
                    {
                        int nameIndex = Random.Range(0, opponentNamesList.Count);

                        if (nameIndex > opponentNamesList.Count) nameIndex = opponentNamesList.Count - 1;

                        racers[i].racerDetails.racerName = opponentNamesList[nameIndex].ToString();

                        opponentNamesList.RemoveAt(nameIndex);
                    }

                    //Ai Racer Name Component
                    if (showRacerNames && racerName)
                    {
                        GameObject _name = (GameObject)Instantiate(racerName);

                        _name.GetComponent<RacerName>().target = racers[i].transform;

                        _name.GetComponent<RacerName>().Initialize();
                    }

                    //Ai Minimap Pointers
                    if (showRacerPointers && opponentPointer)
                    {
                        GameObject o_pointer = (GameObject)Instantiate(opponentPointer);

                        o_pointer.GetComponent<RacerPointer>().target = racers[i].transform;
                    }

                    //Ai Difficulty
                    racers[i].GetComponent<OpponentControl>().SetDifficulty(aiDifficulty);
                }

				if (showStartingGrid == true)
					racers[i].GetComponent<VehicleController>().Active = false;
				else
					racers[i].GetComponent<VehicleController>().Active = true;
			}
        }
		
		//TODO
		public IEnumerator Countdown(float delay)
        {
            //TimeTrail 시 AutoMode 진입 방지
            //if (_raceType == RaceType.TimeTrial)
            //    yield break;

            //Set the race state to racing
            SwitchRaceState(RaceState.Racing);

            //Update cameras
            CameraManager.instance.ActivatePlayerCamera();

            //Check whether music should be played now
            if (SoundManager.instance.musicStart == SoundManager.MusicStart.BeforeCountdown)
                SoundManager.instance.StartMusic();

			Statistics[] racers = GameObject.FindObjectsOfType(typeof(Statistics)) as Statistics[];

			//for (int i = 0; i < racers.Length; i++)
			//	racers[i].GetComponent<Rigidbody>().isKinematic = true;

			//wait for (countdown delay) seconds
			yield return new WaitForSeconds(delay);

            //set total countdown time
            currentCountdownTime = countdownFrom + 1;

            startCountdown = true;

			while (startCountdown == true)
            {
                countdownTimer -= Time.deltaTime;

                if (currentCountdownTime >= 1)
                {
                    if (countdownTimer < 0.01f)
                    {
                        currentCountdownTime -= 1;

                        countdownTimer = 1;

                        if (currentCountdownTime > 0)
                        {
                            RaceUI.instance.SetCountDownText(currentCountdownTime.ToString());

                            SoundManager.instance.PlayDefaultSound(SoundManager.instance.defaultSounds.countdownSound);
                        }
                    }
					if (currentCountdownTime == 2 && showStartingGrid == true)
						ReadyToRace();
				}
                else
                {
                    //Display GO! and call StartRace();
                    startCountdown = false;

					RaceUI.instance.SetCountDownText("GO!");

                    SoundManager.instance.PlayDefaultSound(SoundManager.instance.defaultSounds.startRaceSound);

					StartRace();

                    //Wait for 1 second and hide the text.
                    yield return new WaitForSeconds(1);

                    RaceUI.instance.SetCountDownText(string.Empty);
                }

                yield return null;
            }
        }

		private void ReadyToRace()
		{
			StartCoroutine(EngineStart());
		}

		private IEnumerator EngineStart()
		{
			yield return new WaitForSeconds(1.0f);

			Statistics[] racers = GameObject.FindObjectsOfType(typeof(Statistics)) as Statistics[];

			for (int i = 0; i < racers.Length; i++)
			{
				racers[i].GetComponent<VehicleController>().Active = true;
				//racers[i].GetComponent<Rigidbody>().isKinematic = false;
			}
		}


        void Update()
        {
            //Handle Elimination race times
            if (_raceType == RaceType.Elimination)
                CalculateEliminationTime();

			if (_raceType == RaceType.FuelLimit)
			{
				CheckFuelLimit();
			}
        }
		// TODO
        public void StartRace()
        {
            //enable cars to start racing
            Statistics[] racers = GameObject.FindObjectsOfType(typeof(Statistics)) as Statistics[];

            foreach (Statistics go in racers)
            {
				

				if (_raceType == RaceType.Elimination)
                    eliminationList.Add(go);
            }

            //Start replay recording
            if (enableReplay && GetComponent<ReplayManager>())
                GetComponent<ReplayManager>().GetRacersAndStartRecording(racers);

            //Check whether music should be played now
            if (SoundManager.instance && SoundManager.instance.musicStart == SoundManager.MusicStart.AfterCountdown)
                SoundManager.instance.StartMusic();

            raceStarted = true;
        }

        public void EndRace(int rank)
        {
            StartCoroutine(EndRaceRoutine());

            raceCompleted = true;

			CalculateRaceRewards(rank);

            if (ReplayManager.instance)
                ReplayManager.instance.StopRecording();
        }

        IEnumerator EndRaceRoutine()
        {
            RaceUI.instance.DisableRacePanelChildren();

            RaceUI.instance.SetFinishedText("RACE COMPLETED");

            yield return new WaitForSeconds(3.0f);

            if (autoStartReplay)
                AutoStartReplay();

            SwitchRaceState(RaceState.Complete);
        }

        void CalculateRaceRewards(int pos)
        {
            if (raceRewards.Count >= pos)
            {
                //Give currency
                if (raceRewards[pos - 1].currency > 0)
                {
                    PlayerData.AddCurrency(raceRewards[pos - 1].currency);
                    RaceUI.instance.SetRewardText(raceRewards[pos - 1].currency.ToString("N0"), "", "");
                    Debug.Log("Reward Currency : " + raceRewards[pos - 1].currency);
                }

                //Vehicle Unlock
                if (raceRewards[pos - 1].vehicleUnlock != "" && !PlayerPrefs.HasKey(raceRewards[pos - 1].vehicleUnlock))
                {
                    PlayerData.Unlock(raceRewards[pos - 1].vehicleUnlock);
                    RaceUI.instance.SetRewardText("", raceRewards[pos - 1].vehicleUnlock, "");
                    Debug.Log("Reward Vehicle : " + raceRewards[pos - 1].vehicleUnlock);
                }

                //Track Unlock
                if (raceRewards[pos - 1].trackUnlock != "" && !PlayerPrefs.HasKey(raceRewards[pos - 1].trackUnlock))
                {
                    PlayerData.Unlock(raceRewards[pos - 1].trackUnlock);
                    RaceUI.instance.SetRewardText("", "", raceRewards[pos - 1].trackUnlock);
                    Debug.Log("Reward Track : " + raceRewards[pos - 1].trackUnlock);
                }
            }
        }

        public void PauseRace()
        {
            //No point for pausing in completed or starting grid states
            if (raceCompleted || _raceState == RaceState.StartingGrid) return;


            if (_raceState == RaceState.Paused)
            {
                //Handle un-pausing
                SwitchRaceState(RaceState.Racing);

                Time.timeScale = 1.0f;

                SoundManager.instance.SetVolume();

            }
            else
            {
                //Handle pausing
                SwitchRaceState(RaceState.Paused);

                Time.timeScale = 0.0f;

                AudioListener.volume = 0.0f;
            }
        }

		void CheckFuelLimit()
		{
			if (currentPlayer.GetComponent<VehicleController>().fuel.amount > 0)
				return;

			KnockoutRacer(currentPlayer.GetComponent<Statistics>());
		}

		void CalculateEliminationTime()
        {
            if (!raceStarted || _raceState == RaceState.Complete) return;

            eliminationCounter -= Time.deltaTime;

            if (eliminationCounter <= 0)
            {
                eliminationCounter = eliminationTime;

                if (RankManager.instance.currentRacers > 1) { KnockoutRacer(GetLastPlace()); }

                //end the race after all opponent racers have been eliminated
                AllOpponentsEliminated();
            }
        }

        //Used to knockout a racer
        public void KnockoutRacer(Statistics racer)
        {
            racer.knockedOut = true;

            if (racer.tag == "Player")
            {
				switch (_raceType)
				{
					case RaceType.Elimination:
					case RaceType.LapKnockout:
					{
						SwitchRaceState(RaceState.KnockedOut);

						racer.AIMode();

						//RaceUI Fail Race Panel Config
						string title = (_raceType == RaceType.Elimination) ? "ELIMINATED" : (_raceType == RaceType.LapKnockout) ? "KNOCKED OUT" : "TIMED OUT";
						string reason = (_raceType == RaceType.Elimination) ? "You were eliminated from the race." : (_raceType == RaceType.LapKnockout) ? "You were knocked out of the race." : "You ran out of time.";
						RaceUI.instance.SetFailRace(title, reason);

						if (showRaceInfoMessages)
						{
							string keyword = "";

							keyword = (_raceType == RaceType.Elimination) ? " eliminated." : (_raceType == RaceType.LapKnockout) ? " knocked out." : " timed out.";

							RaceUI.instance.ShowRaceInfo(racer.racerDetails.racerName + keyword, 2.0f, Color.white);
						}

						//ChangeLayer(racer.transform, "IgnoreCollision");

						RankManager.instance.RefreshRacerCount();

						break;
					}
					case RaceType.FuelLimit:
					{
						SwitchRaceState(RaceState.KnockedOut);

						string title = "Race Failed";
						string reason =
							"Exhaust Fuel. \n" +
							"If You Want to Retry, Press Retry Button.\n" +
							"If You Want to Continue, Press '>' Button";
						RaceUI.instance.SetFailRace(title, reason);

						break;
					}
					case RaceType.Checkpoints:
					{
						SwitchRaceState(RaceState.KnockedOut);

						string title = "Race Failed";
						string reason =
							"Run Out of Time. \n" +
							"If You Want to Retry, Press Retry Button.\n" +
							"If You Want to Continue, Press '>' Button";

						currentPlayer.GetComponent<VehicleController>().engine.Toggle();

						RaceUI.instance.SetFailRace(title, reason);

						break;
					}
				}

                //Stop Recording
                if (ReplayManager.instance) { ReplayManager.instance.StopRecording(); }
            }
        }


        //Creates an active ghost car
        public void CreateGhostVehicle(GameObject racer)
        {
            //Destroy any active ghost
            if (activeGhostCar)
            {
                Destroy(activeGhostCar);
            }

            //Create a duplicate ghost car
            GameObject ghost = (GameObject)Instantiate(racer, Vector3.zero, Quaternion.identity);

            ghost.name = "Ghost";

            ghost.tag = "Untagged";

            activeGhostCar = ghost;

            ChangeLayer(ghost.transform, "IgnoreCollision");

            ChangeMaterial(ghost.transform);

            DisableRacerInput(ghost);

            ghost.GetComponent<GhostVehicle>().StartGhost();
        }


        //Format a float to a time string
        public string FormatTime(float time)
        {
            int minutes = (int)Mathf.Floor(time / 60);
            int seconds = (int)time % 60;
            int milliseconds = (int)(time * 100) % 100;

            return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }

        //Loads racer names from a .txt resource file
        public void LoadRacerNames()
        {
            if (!(TextAsset)Resources.Load("RacerNames", typeof(TextAsset)))
            {
                Debug.Log("Names not found! Please add a .txt file named 'RacerNames' with a list of names to /Resources folder.");
                return;
            }
            int lineCount = 0;
            opponentNames = (TextAsset)Resources.Load("RacerNames", typeof(TextAsset));
            nameReader = new StringReader(opponentNames.text);


            string txt = nameReader.ReadLine();
            while (txt != null)
            {
                lineCount++;
                if (opponentNamesList.Count < lineCount)
                {
                    opponentNamesList.Add(txt);
                }
                txt = nameReader.ReadLine();
            }
        }


        //Used to calculate track distance(in meters) & rotate the nodes correctly
        void ConfigureNodes()
        {
            Transform[] m_path = pathContainer.GetComponentsInChildren<Transform>();
            List<Transform> m_pathList = new List<Transform>();
            foreach (Transform node in m_path)
            {
                if (node != pathContainer)
                {
                    m_pathList.Add(node);
                }
            }
            for (int i = 0; i < m_pathList.Count; i++)
            {
                if (i < m_pathList.Count - 1)
                {
                    m_pathList[i].transform.LookAt(m_pathList[i + 1].transform);
                }
                else
                {
                    m_pathList[i].transform.rotation = Quaternion.Euler(m_pathList[i].transform.position - m_pathList[i].transform.position);
                }
            }

            raceDistance = pathContainer.GetComponent<WaypointCircuit>().distances[pathContainer.GetComponent<WaypointCircuit>().distances.Length - 1];
        }

        //used to respawn a racer
        public void RespawnRacer(Transform racer, Transform node, float ignoreCollisionTime)
        {
            if (raceStarted)
                StartCoroutine(Respawn(racer, node, ignoreCollisionTime));
        }

        IEnumerator Respawn(Transform racer, Transform node, float ignoreCollisionTime)
        {
            //Flip the car over and place it at the last passed node
            racer.rotation = Quaternion.LookRotation(racer.forward);
            racer.GetComponent<Rigidbody>().velocity = Vector3.zero;
            racer.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            racer.position = new Vector3(node.position.x, node.position.y + 2.5f, node.position.z);
            racer.rotation = node.rotation;

			bRespawn = true;

			//yield return null;
			yield return new WaitForSeconds(ignoreCollisionTime);

			bRespawn = false;
            //ChangeLayer(racer, "IgnoreCollision");
            //yield return new WaitForSeconds(ignoreCollisionTime);
            //ChangeLayer(racer, "Default");
        }

        //used to change a racers layer to "ignore collision" after being knocked out & on respawn
        public void ChangeLayer(Transform racer, string LayerName)
        {
            return;

            //for (int i = 0; i < racer.childCount; i++)
            //{
            //    racer.GetChild(i).gameObject.layer = LayerMask.NameToLayer(LayerName);
            //    ChangeLayer(racer.GetChild(i), LayerName);
            //}
        }

        //used to change a racers material when creating a ghost car
        public void ChangeMaterial(Transform racer)
        {
            Transform[] m = racer.GetComponentsInChildren<Transform>();

            foreach (Transform t in m)
            {
                if (t.GetComponent<Renderer>())
                {
                    //If the vehicle only uses one material
                    if (t.GetComponent<Renderer>().materials.Length == 1)
                    {
                        if (!useGhostMaterial)
                        {
                            Material instance = t.gameObject.GetComponent<Renderer>().material;
                            instance.shader = (ghostShader) ? ghostShader : Shader.Find("Transparent/Diffuse");
                            Color col = instance.color;
                            col.a = ghostAlpha;
                            instance.color = col;
                            t.gameObject.GetComponent<Renderer>().material = instance;
                        }
                        else
                        {
                            t.gameObject.GetComponent<Renderer>().material = ghostMaterial;
                        }

                    }
                    else
                    {
                        //If the vehicle uses more than one material
                        Material[] instances = new Material[t.GetComponent<Renderer>().materials.Length];
                        Color[] col = new Color[t.GetComponent<Renderer>().materials.Length];

                        for (int i = 0; i < instances.Length; i++)
                        {
                            if (!useGhostMaterial)
                            {
                                instances[i] = t.gameObject.GetComponent<Renderer>().materials[i];
                                instances[i].shader = ghostShader;
                                col[i] = instances[i].color;
                                col[i].a = ghostAlpha;
                                instances[i].color = col[i];
                                t.gameObject.GetComponent<Renderer>().materials[i] = instances[i];
                            }
                            else
                            {
                                instances[i] = ghostMaterial;
                                t.gameObject.GetComponent<Renderer>().materials = instances;
                            }
                        }
                    }
                }
            }
        }


        //Used to disable input for when viewing a replay or for a ghost car
        public void DisableRacerInput(GameObject racer)
        {
            if (racer.GetComponent<PlayerControl>())
                racer.GetComponent<PlayerControl>().enabled = false;


            if (racer.GetComponent<OpponentControl>())
                racer.GetComponent<OpponentControl>().enabled = false;


            if (!racer.GetComponent<Statistics>().finishedRace)
                racer.GetComponent<Statistics>().finishedRace = true;
        }

        public void SwitchRaceState(RaceState state)
        {
            _raceState = state;

            //Update UI
            RaceUI.instance.UpdateUIPanels();

        }

        /// <summary>
        /// Automatically starts the reply by manually setting the appropriate values
        /// </summary>
        void AutoStartReplay()
        {
            if (ReplayManager.instance.TotalFrames <= 0) return;

            StartCoroutine(RaceUI.instance.ScreenFadeOut(0.5f));

            _raceState = RaceState.Replay;

            ReplayManager.instance.replayState = ReplayManager.ReplayState.Playing;

            CameraManager.instance.ActivateCinematicCamera();

            for (int i = 0; i < ReplayManager.instance.racers.Count; i++)
            {
                DisableRacerInput(ReplayManager.instance.racers[i].racer.gameObject);
            }
        }

        // Checks if all racers have finished
        public bool AllRacersFinished()
        {
            bool allFinished = false;

            Statistics[] allRacers = GameObject.FindObjectsOfType(typeof(Statistics)) as Statistics[];

            for (int i = 0; i < allRacers.Length; i++)
            {
                if (allRacers[i].finishedRace)
                    allFinished = true;
                else
                    allFinished = false;
            }

            return allFinished;
        }

        void AllOpponentsEliminated()
        {

            for (int i = 0; i < eliminationList.Count; i++)
            {

                if (eliminationList[i].knockedOut)
                {
                    eliminationList.Remove(eliminationList[i]);
                }

                if (eliminationList.Count == 1 && eliminationList[0].gameObject.tag == "Player")
                {
                    eliminationList[0].FinishRace();
                }
            }
        }

        public Statistics GetLastPlace()
        {
            return RankManager.instance.racerRanks[RankManager.instance.currentRacers - 1].racer.GetComponent<Statistics>();
        }

        private int SetValue(int val, int otherVal)
        {
            int myVal = val;

            if (val > otherVal)
            {
                myVal = otherVal;
            }
            else if (val <= 0)
            {
                myVal = 1;
            }

            return myVal;
        }


        string ReplaceString(string stringValue, string toRemove)
        {
            return stringValue.Replace(toRemove, "");
        }
    }
}
