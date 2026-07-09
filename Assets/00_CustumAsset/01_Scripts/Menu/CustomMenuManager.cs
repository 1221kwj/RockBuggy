using System;
using System.Collections;
using RGSK;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

public class CustomMenuManager : MonoBehaviour
{
    [System.Serializable]
    public class MenuVehicle
    {
        [Header("Details")]
        public string name;
        public string resourceName; // Make sure this string matches the coresponding vehicle name in a Reources/PlayerVehicles folder!
        public Transform vehicle;        
        public int price;
        public bool unlocked;
        public Material vehicleBody;
        public Material VehicleRims;
        public int DefaultBodyColorID;

        [Header("Specs")]
        [Range(0, 1000)]
        public int defaultSpeed;
        [Range(0, 1000)]
        public int defaultAcceleration;
        [Range(0, 1000)]
        public int defaultHandling;
        [Range(0, 1000)]
        public int defaultBraking;

        [Header("Custom")]
        public VehicleUpgrade upgrade;
    }

    private enum UpgradeState { None, Vehicle, Speed, Accel, Handle, Brake }
    [System.Serializable]
    public class VehicleUpgrade
    {
        public int vehicleLevel;
        public int maxLevel = 5;
        public int speedLevel;
        public int accelerationLevel;
        public int handlingLevel;
        public int brakingLevel;
        public int levelupValue;
    }



    [System.Serializable]
    public class MenuTrack
    {
        public string name;
        public string trackLength;
        public string sceneName;
        public Sprite image;
        public Sprite nightImage;
        public RaceManager.RaceType[] CanPlayRaceTypes;
        public RaceManager.RaceType raceType = RaceManager.RaceType.Circuit;
        public OpponentControl.AiDifficulty aiDifficulty = OpponentControl.AiDifficulty.Meduim;
        public RaceManager.RaceTime raceTime = RaceManager.RaceTime.Day;
        public int laps = 3;
        public int aiCount = 4;
        public bool useAI;
        public bool useLap;
        public int timeLimit;
        public int price;
        public bool unlocked;
    }

    #region Customization Classes
    [System.Serializable]
    public class CustomizeItem
    {
        public string name;
        public int ID;
        public int price;
        public Text priceText;
        [HideInInspector] public bool unlocked;
    }

    [System.Serializable]
    public class VisualUpgrade : CustomizeItem
    {
        public BodyColorAndRims[] visualUpgrade;
    }

    [System.Serializable]
    public class BodyColorAndRims
    {
        public string vehicle_name;
        public Texture texture;
    }    
    #endregion

    public enum State { Main, Tuning, TrackSelect, Settings, Loading , RaceResult, Tutorial }
    private State _state;
    public State state { get { return _state; } set { _state = value; } }

    [Header("Vehicle settings")]
    public MenuVehicle[] menuVehicles;

    [Header("Track Settings")]
    public MenuTrack[] menuTracks;

    [Header("Customize Settings")]
    public VisualUpgrade[] bodyColors;
    public VisualUpgrade[] rims;

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject tuningPanel;
    public GameObject modeSelectPanel; //trackSelectPanel
    public GameObject vehicleStats;    
    public GameObject settingsPanel;
    public GameObject promptPanel;
    public GameObject loadingPanel;
    public GameObject tutorialPanel;

    [Header("Top Panel UI")]
    public Text playerCurrency;
    public Text playerNickName;
    public Image playerIcon;
    public Text playerLevelText;
    public Image playerExpBar;
    public Button back_setting_Button;
    public Text menuState;
    public Sprite optionImg;
    public Sprite backImg;

    [Header("Main Panel UI")]
    public Text vehiclePrice;
    //public Text upgrade_buycar_text;
    public Button modeSelectButton;

    [Header("Vehicle Select UI in Main Panel")]
    public Text vehicleName;
    public Button upgrade_buycar_Button;
	public Button mapSelectButton;
    public Image speed, accel, handling, braking;

    public enum CustomState { Color, Upgrade }
    [Header("Tuning UI")]
    public CustomState customState;// = CustomState.Color;
    public GameObject colorCustomPanel;
    public GameObject upgradeCustomPanel;
    public Button colorCustomButton;
    public Button upgradeCustomButton;
    //color
    public Button colorBuyEqiupButton;
    public Text colorPriceText;
    //upgrade
    public Button upgradeButton;
    public Text upgradePriceText;
    public Sprite unupgradableSprite;
    public Image vehicleLevelImg;
    public Text vehicleLevelText;
    public Text speedValueText;
    public Text accelValueText;
    public Text handleValueText;
    public Text brakeValueText;
    private int incartCr, bodyColPrice, rimPrice, upgradePrice, selectedColorID, selectedRimID, selectedUpgradeID;
    private UpgradeState upgradeState;
    

    [Header("Track Select UI")]
    public Image trackImage;
    public Text trackName, raceType, lapCount, aiCount, aiDifficulty, raceTime, bestTime, trackLength, timeLimitText;
    public Button race_buy_Button;
    public Text race_buy_Text;

    [Header("Settings UI")]
    public InputField playerName;
    public Slider masterVolume;
    public Dropdown graphicLevel;
    public Toggle mobileTouchSteer, mobileGyroSteer, mobileAutoAcceleration, mobileButtonAcceleration;
    public bool applyExpensiveGraphicChanges = false;

    [Header("Loading UI")]
    public Image loadingBar;

    [Header("Prompt Panel UI")]
    public Text promptTitle;
    public Text promptText;
    public Button accept, cancel;

    [Header("Misc UI")]
    public Text itemPrice;
    public Image locked;
    public Image cart;
    public Button nextArrow, prevArrow;

    [Header("Extra Settings")]
    public Sprite upgradeImg;
    public Sprite tuningImg;
    public Sprite buyImg;
    public Sprite playImg;
    public Sprite applyImg;
    public GameObject modelsRoot;
    public bool autoRotateVehicles = true;
    public bool rotateVehicleByDrag = true;
    public float rotateSpeed = 5.0f;
    public int maxOpponents = 5;

	public AdMobBannerView bannerView;
    //[Range(1, 7)] public int raceTypes = 7;

    //Private vars
    private int vehicleIndex;
    private int prevVehicleIndex;
    private int trackIndex;
    private int raceTypeIndex = 1;
    private int raceTimeIndex = 0;
    private int aiDiffIndex = 1;
    private AsyncOperation async;
    private State previousState;
    private bool raycastTarget;
    private bool _autoRotate; // cache
    private float rotateDir = 1;
    private Texture lastColTex;
    private Texture lastRimTex;
    private Vector2 lastTouchPosition = new Vector2();
    private int[] acc_EXP = { 0, 14, 32, 58, 97, 153, 231, 336, 472, 644, 856, 1113, 1420, 1782, 2203, 2688, 3241, 3867, 4571, 5357, 6230, 7195, 8257, 9420, 10689, 12068, 13562, 15176, 16914, 18781 };
    private string[] rank_Name = {"Beginner A","Beginner B","Beginner C","Rookie A","Rookie B","Rookie C","Junior A","Junior B","Junior C","Middle A","Middle B","Middle C","Senior A","Senior B","Senior C","Semi Pro A","Semi Pro B","Semi Pro C","Semi Pro D","Semi Pro E","Pro A","Pro B","Pro C","Pro D","Pro E","Legend A","Legend B","Legend B","Legend C","Legend E"};
    private int playerExp;
    private static CustomMenuManager _Instance;
    public static CustomMenuManager Instance
    {
        get
        {
            if (_Instance != null)
                return _Instance;
            else
                return null;
        }
    }

    void Awake()
    {
        LoadValues();
    }

    void Start()
    {
        if (_Instance == null) _Instance = this;

		var _ratio = (float)Screen.height / Screen.width;

		Screen.SetResolution(720, (int)(720f * _ratio), true);

//#if UNITY_EDITOR
//		PlayerData.AddCurrency(10000);

//#endif

		state = (PlayerPrefs.HasKey("tutorialPassed"))? ((PlayerPrefs.GetInt("tutorialPassed") == 0) ? State.Tutorial : State.Main) : State.Tutorial;

        CycleVehicles();

        PlayerLevelCalculate();

        if (masterVolume) masterVolume.onValueChanged.AddListener(delegate { SetMasterVolFromSlider(); });
        if (playerName) playerName.onEndEdit.AddListener(delegate { SetPlayerNameFromInputField(); });
        if (playerNickName) playerNickName.text = playerName.text;
        if (graphicLevel) graphicLevel.onValueChanged.AddListener(delegate { GetGrahicLevelFromDropdown(); });
        if (graphicLevel) graphicLevel.value = QualitySettings.GetQualityLevel();
        if (mobileTouchSteer) mobileTouchSteer.onValueChanged.AddListener(delegate { ToggleTouchControl(); });
        if (mobileGyroSteer) mobileGyroSteer.onValueChanged.AddListener(delegate { ToggleGyroControl(); });
        if (mobileAutoAcceleration) mobileAutoAcceleration.onValueChanged.AddListener(delegate { ToggleAutoAccel(); });
        if (mobileButtonAcceleration) mobileButtonAcceleration.onValueChanged.AddListener(delegate { ToggleButtonAccel(); });

        //강화 관련 부분 불러오기, maxLevel 및 levelupValue는 에디터에서 입력한 값을 기준으로 삼으며 기존 데이터를 불러오지 않음
        for (int i  = 0; i < menuVehicles.Length; i++)
        {
            if (PlayerPrefs.HasKey("vehicleLevel" + menuVehicles[vehicleIndex].name))
                menuVehicles[vehicleIndex].upgrade.vehicleLevel = PlayerPrefs.GetInt(string.Concat("vehicleLevel", menuVehicles[vehicleIndex].name));
            if (PlayerPrefs.HasKey("speedLevel" + menuVehicles[vehicleIndex].name))
                menuVehicles[vehicleIndex].upgrade.speedLevel = PlayerPrefs.GetInt(string.Concat("speedLevel", menuVehicles[vehicleIndex].name));
            if (PlayerPrefs.HasKey("accelLevel" + menuVehicles[vehicleIndex].name))
                menuVehicles[vehicleIndex].upgrade.accelerationLevel = PlayerPrefs.GetInt(string.Concat("accelLevel", menuVehicles[vehicleIndex].name));
            if (PlayerPrefs.HasKey("handleLevel" + menuVehicles[vehicleIndex].name))
                menuVehicles[vehicleIndex].upgrade.handlingLevel = PlayerPrefs.GetInt(string.Concat("handleLevel", menuVehicles[vehicleIndex].name));
            if (PlayerPrefs.HasKey("brakeLevel" + menuVehicles[vehicleIndex].name))
                menuVehicles[vehicleIndex].upgrade.brakingLevel = PlayerPrefs.GetInt(string.Concat("brakeLevel", menuVehicles[vehicleIndex].name));
        }

        _autoRotate = autoRotateVehicles;
        selectedColorID = -1;
        selectedRimID = -1;
        selectedUpgradeID = -1;        
    }

    public void PlayerLevelCalculate()
    {
        if(PlayerPrefs.HasKey("PlayerLevel"))
        {
            playerExp = PlayerPrefs.GetInt("PlayerLevel");
            for(int i = 0; i < acc_EXP.Length; i++)
            {
                if (playerExp > acc_EXP[i])
                    continue;
                else if (playerExp == acc_EXP[i] || i == 0)
                {
                    playerLevelText.text = rank_Name[i];
                    playerExpBar.fillAmount = 0;
                }
                else
                {
                    if (i != 0)
                    {
                        playerLevelText.text = rank_Name[i];
                        playerExpBar.fillAmount = (float)(playerExp - acc_EXP[i - 1]) / (float)(acc_EXP[i] - acc_EXP[i - 1]);
                        break;
                    }
                }
            }
        }
        else
        {
            playerLevelText.text = rank_Name[0];
            playerExpBar.fillAmount = 0;
            PlayerPrefs.SetInt("PlayerLevel", 0);
        }
    }

    public void TutorialStart()
    {
        state = State.Tutorial;

        UpdateUI();
    }

    public void TutorialPass()
    {
        state = State.Main;
        if (tutorialPanel) tutorialPanel.SetActive(false);
        PlayerPrefs.SetInt("tutorialPassed", 1);
        UpdateUI();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Back();

        RotateVehicle();

        LerpStats();
    }


    void CycleVehicles()
    {
        // Cycle between vehicles based on the "vehicleIndex" value
        for (int i = 0; i < menuVehicles.Length; i++)
        {
            if (vehicleIndex == i)
            {
                menuVehicles[i].vehicle.rotation = menuVehicles[prevVehicleIndex].vehicle.rotation;
                menuVehicles[i].vehicle.gameObject.SetActive(true);
                UpdateUI();
            }
            else
            {
                menuVehicles[i].vehicle.gameObject.SetActive(false);
            }
        }
    }

    void CycleTracks()
    {
        // Cycle between tracks based on the "trackIndex" value
        UpdateUI();
    }

    void UpdateUI()
    {
        if (playerNickName) playerNickName.text = PlayerPrefs.GetString("PlayerName");

        if (playerCurrency) playerCurrency.text = string.Concat("$ ", PlayerData.currency.ToString("N0"));

        //if (cart) cart.enabled = state == State.Customize_Upgrade;

        if (nextArrow) nextArrow.gameObject.SetActive(state == State.Main);

        if (prevArrow) prevArrow.gameObject.SetActive(state == State.Main);

        if (vehicleStats) vehicleStats.SetActive(state == State.Main);

        if (back_setting_Button)
        {
            back_setting_Button.onClick.RemoveAllListeners();
            if (state == State.Main)
            {
                back_setting_Button.onClick.AddListener(Settings);
                ((Image)back_setting_Button.targetGraphic).sprite = optionImg;
            }
            else
            {
                back_setting_Button.onClick.AddListener(Back);
                ((Image)back_setting_Button.targetGraphic).sprite = backImg;
            }
        }

        switch (state)
        {
			
            case State.Main:
                mainPanel.SetActive(true);
                tuningPanel.SetActive(false);
                modeSelectPanel.SetActive(false);
                //customizePanel.SetActive(false);
                settingsPanel.SetActive(false);
                loadingPanel.SetActive(false);
                if (tutorialPanel) tutorialPanel.SetActive(false);

                if (vehicleName) vehicleName.text = menuVehicles[vehicleIndex].name;

                if (vehiclePrice) vehiclePrice.text = menuVehicles[vehicleIndex].unlocked ? string.Empty : "$ " + menuVehicles[vehicleIndex].price.ToString("N0");

                //if (upgrade_buycar_text) upgrade_buycar_text.text = menuVehicles[vehicleIndex].unlocked ? "UPGRADE" : "BUY CAR";

                if (upgrade_buycar_Button)
                {
                    ((Image)upgrade_buycar_Button.targetGraphic).sprite = menuVehicles[vehicleIndex].unlocked ? tuningImg : buyImg;

					if (menuVehicles[vehicleIndex].unlocked == false)
						upgrade_buycar_Button.interactable = menuVehicles[vehicleIndex].price <= PlayerData.currency ? true : false;
					else
						upgrade_buycar_Button.interactable = true;

					mapSelectButton.interactable = menuVehicles[vehicleIndex].unlocked ? true : false;

                    upgrade_buycar_Button.onClick.RemoveAllListeners();
                    if (menuVehicles[vehicleIndex].unlocked)
                        upgrade_buycar_Button.onClick.AddListener(TuningSelect);
                    else
                        upgrade_buycar_Button.onClick.AddListener(Buy);
                }                

                if (locked) locked.enabled = !menuVehicles[vehicleIndex].unlocked;

                if (menuState) menuState.text = "VEHICLE SELECT";

                break;

            case State.Tuning:
                mainPanel.SetActive(false);
                tuningPanel.SetActive(true);
                modeSelectPanel.SetActive(false);
                //customizePanel.SetActive(false);
                settingsPanel.SetActive(false);
                loadingPanel.SetActive(false);
                if (tutorialPanel) tutorialPanel.SetActive(false);

                if (colorCustomPanel) colorCustomPanel.SetActive(customState == CustomState.Color);
                if (colorCustomButton) colorCustomButton.interactable = !(customState == CustomState.Color);

                if (upgradeCustomPanel) upgradeCustomPanel.SetActive(customState == CustomState.Upgrade);
                if (upgradeCustomButton) upgradeCustomButton.interactable = !(customState == CustomState.Upgrade);

                incartCr = bodyColPrice + rimPrice + upgradePrice;

                #region colorCustom
                if (customState == CustomState.Color)
                {
                    CheckForUnlockedCustomizations();
                    if (menuVehicles[vehicleIndex].DefaultBodyColorID > 0) bodyColors[menuVehicles[vehicleIndex].DefaultBodyColorID - 1].unlocked = true;

                    if (colorBuyEqiupButton) colorBuyEqiupButton.gameObject.SetActive(selectedColorID >= 0);

                    if (colorPriceText) colorPriceText.gameObject.SetActive(selectedColorID >= 0);

                    if (colorCustomPanel) colorCustomPanel.SetActive(true);

                    if (colorPriceText && selectedColorID >= 0) colorPriceText.text = (!bodyColors[selectedColorID].unlocked) ? "$ " + incartCr.ToString("N0") : string.Empty;

                    //if (colorBuyEqiupText && selectedColorID >= 0) colorBuyEqiupText.text = (!bodyColors[selectedColorID].unlocked) ? "Buy" : "Equip";

                    if (colorBuyEqiupButton && selectedColorID >= 0)
                    {
                        ((Image)colorBuyEqiupButton.targetGraphic).sprite = (!bodyColors[selectedColorID].unlocked) ? buyImg : applyImg;

						if (bodyColors[selectedColorID].unlocked == false)
							colorBuyEqiupButton.interactable = bodyColors[selectedColorID].price <= PlayerData.currency ? true : false;
						else
							colorBuyEqiupButton.interactable = true;

						colorBuyEqiupButton.onClick.RemoveAllListeners();
                        if (bodyColors[selectedColorID].unlocked)
                            colorBuyEqiupButton.onClick.AddListener(ApplyColor);
                        else
                            colorBuyEqiupButton.onClick.AddListener(Buy);
                    }
                }
                #endregion

                #region upgradeCustom
                if (customState == CustomState.Upgrade)
                {
                    if (upgradeButton)
                    {
                        ((Image)upgradeButton.targetGraphic).sprite = unupgradableSprite;

                        upgradeButton.interactable = (upgradeState == UpgradeState.Vehicle);
                    }

                    if (upgradePriceText)
                    {
                        upgradePriceText.gameObject.SetActive(upgradeState != UpgradeState.None);
                        upgradePriceText.text = string.Empty;
                    }

                    if (vehicleLevelText) vehicleLevelText.text =
                            (menuVehicles[vehicleIndex].upgrade.vehicleLevel < menuVehicles[vehicleIndex].upgrade.maxLevel) ?
                            "Vehicle Level " + menuVehicles[vehicleIndex].upgrade.vehicleLevel : "Vehcle Level Max";

                    if (vehicleLevelImg) vehicleLevelImg.fillAmount = (float)menuVehicles[vehicleIndex].upgrade.vehicleLevel / (float)menuVehicles[vehicleIndex].upgrade.maxLevel;

                    if (speedValueText) speedValueText.text = (menuVehicles[vehicleIndex].upgrade.speedLevel
                                                             * menuVehicles[vehicleIndex].upgrade.levelupValue).ToString();

                    if (accelValueText) accelValueText.text = (menuVehicles[vehicleIndex].upgrade.accelerationLevel
                                                             * menuVehicles[vehicleIndex].upgrade.levelupValue).ToString();

                    if (handleValueText) handleValueText.text = (menuVehicles[vehicleIndex].upgrade.handlingLevel
                                                             * menuVehicles[vehicleIndex].upgrade.levelupValue).ToString();

                    if (brakeValueText) brakeValueText.text = (menuVehicles[vehicleIndex].upgrade.brakingLevel
                                                             * menuVehicles[vehicleIndex].upgrade.levelupValue).ToString();

                    if (upgradeState != UpgradeState.None)
                    {
                        switch(upgradeState)
                        {
                            case UpgradeState.Vehicle:
							{
								if (menuVehicles[vehicleIndex].upgrade.vehicleLevel < menuVehicles[vehicleIndex].upgrade.maxLevel)
								{
									((Image)upgradeButton.targetGraphic).sprite = upgradeImg;
									upgradeButton.interactable = incartCr <= PlayerData.currency ? true : false;
									upgradePriceText.gameObject.SetActive(true);
									upgradePriceText.text = "$ " + incartCr.ToString("N0");
								}
								else
									upgradeButton.interactable = false;
								break;
							}
                            case UpgradeState.Speed:
							{
								if (menuVehicles[vehicleIndex].upgrade.speedLevel < menuVehicles[vehicleIndex].upgrade.vehicleLevel)
								{
									((Image)upgradeButton.targetGraphic).sprite = upgradeImg;
									upgradeButton.interactable = incartCr <= PlayerData.currency ? true : false;
									upgradePriceText.gameObject.SetActive(true);
									upgradePriceText.text = "$ " + incartCr.ToString("N0");
								}
								else
									upgradeButton.interactable = false;
								break;
							}
                            case UpgradeState.Accel:
							{
								if (menuVehicles[vehicleIndex].upgrade.accelerationLevel < menuVehicles[vehicleIndex].upgrade.vehicleLevel)
								{
									((Image)upgradeButton.targetGraphic).sprite = upgradeImg;
									upgradeButton.interactable = incartCr <= PlayerData.currency ? true : false;
									upgradePriceText.gameObject.SetActive(true);
									upgradePriceText.text = "$ " + incartCr.ToString("N0");
								}
								else
									upgradeButton.interactable = false;
								break;
							}
                            case UpgradeState.Handle:
							{
								if (menuVehicles[vehicleIndex].upgrade.handlingLevel < menuVehicles[vehicleIndex].upgrade.vehicleLevel)
								{
									((Image)upgradeButton.targetGraphic).sprite = upgradeImg;
									upgradeButton.interactable = incartCr <= PlayerData.currency ? true : false;
									upgradePriceText.gameObject.SetActive(true);
									upgradePriceText.text = "$ " + incartCr.ToString("N0");
								}
								else
									upgradeButton.interactable = false;
								break;
							}
                            case UpgradeState.Brake:
							{
								if (menuVehicles[vehicleIndex].upgrade.brakingLevel < menuVehicles[vehicleIndex].upgrade.vehicleLevel)
								{
									((Image)upgradeButton.targetGraphic).sprite = upgradeImg;
									upgradeButton.interactable = incartCr <= PlayerData.currency ? true : false;
									upgradePriceText.gameObject.SetActive(true);
									upgradePriceText.text = "$ " + incartCr.ToString("N0");
								}
								else
									upgradeButton.interactable = false;
								break;
							}
                        }
                    }
                }
                #endregion

                    if (menuState) menuState.text = "TUNING";

                break;

            case State.TrackSelect:
                mainPanel.SetActive(false);
                tuningPanel.SetActive(false);
                modeSelectPanel.SetActive(true);
                //customizePanel.SetActive(false);
                settingsPanel.SetActive(false);
                loadingPanel.SetActive(false);
                if (tutorialPanel) tutorialPanel.SetActive(false);

                if (trackName) trackName.text = menuTracks[trackIndex].name;

                if (trackLength) trackLength.text = menuTracks[trackIndex].trackLength;

                if (trackImage && menuTracks[trackIndex].image)
                {
                    if(menuTracks[trackIndex].raceTime == RaceManager.RaceTime.Night && menuTracks[trackIndex].nightImage)
                        trackImage.sprite = menuTracks[trackIndex].nightImage;
                    else
                        trackImage.sprite = menuTracks[trackIndex].image;
                }

                if (raceType) raceType.text = (menuTracks[trackIndex].raceType != RaceManager.RaceType.Checkpoints)?menuTracks[trackIndex].raceType.ToString() : "TimeLimit";

                if (timeLimitText) timeLimitText.transform.parent.gameObject.SetActive(false);

                if (lapCount)
                {
                    if (!menuTracks[trackIndex].useLap
                        || menuTracks[trackIndex].raceType == RaceManager.RaceType.FuelLimit
                        || menuTracks[trackIndex].raceType == RaceManager.RaceType.TimeTrial
                        || menuTracks[trackIndex].raceType == RaceManager.RaceType.Drift
                        || menuTracks[trackIndex].raceType == RaceManager.RaceType.Checkpoints)
                    {
                        lapCount.transform.parent.gameObject.SetActive(false);

                        //제한시간 사용하는 타입들, lap정보 자리에 timeLimit정보 표시하기 위함
                        if (timeLimitText && menuTracks[trackIndex].raceType == RaceManager.RaceType.Checkpoints)
                        {
                            timeLimitText.transform.parent.gameObject.SetActive(true);
                            timeLimitText.text = string.Concat
							(
								(menuTracks[trackIndex].timeLimit / 60).ToString("00"), 
								":",
								(menuTracks[trackIndex].timeLimit % 60).ToString("00"),
								":00"
							);
                        }
                    }
                    else
                    {
                        lapCount.transform.parent.gameObject.SetActive(true);
                        lapCount.text = menuTracks[trackIndex].laps.ToString();
                    }
                }

                if (aiCount)
                {
                    if (!menuTracks[trackIndex].useAI
                        || menuTracks[trackIndex].raceType == RaceManager.RaceType.FuelLimit
                        || menuTracks[trackIndex].raceType == RaceManager.RaceType.TimeTrial
                        || menuTracks[trackIndex].raceType == RaceManager.RaceType.Drift)
                        aiCount.transform.parent.gameObject.SetActive(false);
                    else
                    {
                        aiCount.transform.parent.gameObject.SetActive(true);
                        aiCount.text = menuTracks[trackIndex].aiCount.ToString();
                    }
                }

                //if (aiDifficulty)
                //{
                //    if (!menuTracks[trackIndex].useAI
                //        || menuTracks[trackIndex].raceType == RaceManager.RaceType.FuelLimit
                //        || menuTracks[trackIndex].raceType == RaceManager.RaceType.TimeTrial
                //        || menuTracks[trackIndex].raceType == RaceManager.RaceType.Drift)
                //        aiDifficulty.transform.parent.gameObject.SetActive(false);
                //    else
                //    {
                //        aiDifficulty.transform.parent.gameObject.SetActive(true);
                //        aiDifficulty.text = menuTracks[trackIndex].aiDifficulty.ToString();
                //    }
                //}

                if (raceTime) raceTime.text = menuTracks[trackIndex].raceTime.ToString();

                if (race_buy_Text) race_buy_Text.text = menuTracks[trackIndex].unlocked ? string.Empty : string.Concat("$ ", menuTracks[trackIndex].price.ToString("N0"));

                if (locked) locked.enabled = !menuTracks[trackIndex].unlocked;

                if (race_buy_Button)
                {
                    ((Image)race_buy_Button.targetGraphic).sprite = (menuTracks[trackIndex].unlocked) ? playImg : buyImg;

					if (menuTracks[trackIndex].unlocked == false)
						race_buy_Button.interactable = menuTracks[trackIndex].price <= PlayerData.currency ? true : false;
					else
						race_buy_Button.interactable = true;

                    race_buy_Button.onClick.RemoveAllListeners();
                    if (menuTracks[trackIndex].unlocked)
                        race_buy_Button.onClick.AddListener(Play);
                    else
                        race_buy_Button.onClick.AddListener(Buy);
                }

                //if (race_buy_Text) race_buy_Text.text = (menuTracks[trackIndex].unlocked) ? "START" : "BUY";

                if (bestTime) bestTime.text = (PlayerPrefs.HasKey("BestTime" + menuTracks[trackIndex].sceneName)) ? PlayerPrefs.GetString("BestTime" + menuTracks[trackIndex].sceneName) : "--:--:--";

                if (menuState) menuState.text = "TRACK SELECT";

                break;

            //case State.Customize_Color:
            //    mainPanel.SetActive(false);
            //    tuningPanel.SetActive(true);
            //    modeSelectPanel.SetActive(false);
            //    customizePanel.SetActive(true);
            //    settingsPanel.SetActive(false);
            //    loadingPanel.SetActive(false);

            //    //Calculate the in cart currency
            //    incartCr = bodyColPrice + rimPrice + upgradePrice;

            //    //Fill in the price texts (BODY COLORS)
            //    for (int c = 0; c < bodyColors.Length; c++)
            //    {
            //        if (bodyColors[c].priceText) bodyColors[c].priceText.text = !bodyColors[c].unlocked ? bodyColors[c].price.ToString("N0") : "Owned";
            //    }

            //    //Fill in the price texts (RIMS)
            //    for (int r = 0; r < rims.Length; r++)
            //    {
            //        if (rims[r].priceText) rims[r].priceText.text = !rims[r].unlocked ? rims[r].price.ToString("N0") : "Owned";
            //    }

            //    if (colorsPanel) colorsPanel.SetActive(true);

            //    if (apply) apply.gameObject.SetActive(incartCr <= 0 && selectedColorID >= 0 || incartCr <= 0 && selectedRimID >= 0 || incartCr <= 0 && selectedUpgradeID >= 0);

            //    if (buy) buy.gameObject.SetActive(incartCr > 0);

            //    if (itemPrice) itemPrice.text = "$ " + incartCr.ToString("N0");

            //    if (menuState) menuState.text = "CUSTOMIZE";

            //    break;

            case State.Settings:
                mainPanel.SetActive(false);
                tuningPanel.SetActive(false);
                modeSelectPanel.SetActive(false);
                //customizePanel.SetActive(false);
                settingsPanel.SetActive(true);
                loadingPanel.SetActive(false);
                if (tutorialPanel) tutorialPanel.SetActive(false);

                if (menuState) menuState.text = "SETTINGS";

                break;

            case State.Loading:
                mainPanel.SetActive(false);
                tuningPanel.SetActive(false);
                modeSelectPanel.SetActive(false);
                //customizePanel.SetActive(false);
                settingsPanel.SetActive(false);
                loadingPanel.SetActive(true);
                if (tutorialPanel) tutorialPanel.SetActive(false);

                break;

            case State.Tutorial:
                mainPanel.SetActive(false);
                tuningPanel.SetActive(false);
                modeSelectPanel.SetActive(false);
                //customizePanel.SetActive(false);
                settingsPanel.SetActive(false);
                loadingPanel.SetActive(false);
				if (tutorialPanel)
				{
					bannerView.HiddenAdBanner();
					tutorialPanel.SetActive(true);
				}
                break;
        }
    }

    /// <summary>
    /// Lerps the stat values to suit the selected vehicle
    /// </summary>
    private void LerpStats()
    {
        //Normal Stats
        if (speed) speed.fillAmount = Mathf.Lerp(speed.fillAmount,
            (menuVehicles[vehicleIndex].defaultSpeed + menuVehicles[vehicleIndex].upgrade.speedLevel * menuVehicles[vehicleIndex].upgrade.levelupValue) / 2000.0f,
            Time.deltaTime * 3.0f);

        if (accel) accel.fillAmount = Mathf.Lerp(accel.fillAmount,
            (menuVehicles[vehicleIndex].defaultAcceleration + menuVehicles[vehicleIndex].upgrade.accelerationLevel * menuVehicles[vehicleIndex].upgrade.levelupValue) / 2000.0f,
            Time.deltaTime * 3.0f);

        if (handling) handling.fillAmount = Mathf.Lerp(handling.fillAmount,
            (menuVehicles[vehicleIndex].defaultHandling + menuVehicles[vehicleIndex].upgrade.handlingLevel * menuVehicles[vehicleIndex].upgrade.levelupValue) / 2000.0f,
            Time.deltaTime * 3.0f);

        if (braking) braking.fillAmount = Mathf.Lerp(braking.fillAmount,
            (menuVehicles[vehicleIndex].defaultBraking + menuVehicles[vehicleIndex].upgrade.brakingLevel * menuVehicles[vehicleIndex].upgrade.levelupValue) / 2000.0f,
            Time.deltaTime * 3.0f);
    }


    private void RotateVehicle()
    {
        if (autoRotateVehicles)
        {
            if(modelsRoot == null)
                menuVehicles[vehicleIndex].vehicle.Rotate(0, (rotateSpeed * Time.deltaTime) * rotateDir, 0);
            else
                modelsRoot.transform.Rotate(0, (rotateSpeed * Time.deltaTime) * rotateDir, 0);
        }


        //Rotate by drag raycast check
        if (rotateVehicleByDrag)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Collider[] childTransforms = menuVehicles[vehicleIndex].vehicle.GetComponentsInChildren<Collider>();

                    foreach (Collider t in childTransforms)
                    {
                        if (hit.collider == t)
                        {
                            autoRotateVehicles = false;
                            raycastTarget = true;
                        }
                        else
                        {
                            raycastTarget = false;
                            if (_autoRotate) autoRotateVehicles = true;
                        }
                    }
                }
            }

            if (Input.GetButtonUp("Fire1"))
            {
                Vector3 mPos = Camera.main.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

                if (raycastTarget) rotateDir = (mPos.x < 0.5f) ? 1 : -1;

                if (_autoRotate) autoRotateVehicles = true;

                raycastTarget = false;
            }

            if (!raycastTarget) return;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL

            if(modelsRoot == null)
                menuVehicles[vehicleIndex].vehicle.Rotate(0, -Input.GetAxis("Mouse X"), 0);
            else
                modelsRoot.transform.Rotate(0, -Input.GetAxis("Mouse X"), 0);

#else
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Vector2 fingerPos = Input.GetTouch(0).position;
                if(lastTouchPosition != new Vector2())
                {
                    if(modelsRoot == null)
                        menuVehicles[vehicleIndex].vehicle.Rotate(0, lastTouchPosition.x - fingerPos.x, 0);
                    else
                        modelsRoot.transform.Rotate(0, lastTouchPosition.x - fingerPos.x, 0);
                }
                lastTouchPosition = fingerPos;
            }
            else
                lastTouchPosition = new Vector2();
#endif
        }
    }

    private void ApplyColorCustomization(int bodyCol, int tovehicleIndex)
    {

        bodyColors[bodyCol].unlocked = true;

        //Unclock the color
        if (!PlayerPrefs.HasKey("BodyColor" + bodyColors[bodyCol].ID + menuVehicles[tovehicleIndex].name))
            PlayerPrefs.SetInt("BodyColor" + bodyColors[bodyCol].ID + menuVehicles[tovehicleIndex].name, 1);

        //Save as the vehicle's current color
        PlayerPrefs.SetInt("CurrentBodyColor" + menuVehicles[tovehicleIndex].name, bodyCol);

        try
		{
			//menuVehicles[tovehicleIndex].vehicleBody.mainTexture = bodyColors[bodyCol].visualUpgrade[tovehicleIndex].texture;
			menuVehicles[tovehicleIndex].vehicleBody.SetTexture("_BaseMap", bodyColors[bodyCol].visualUpgrade[tovehicleIndex].texture);
		}
        catch { Debug.LogError("You haven't properly configured color customizations for this vehicle! Ensure you have assigned a material for your vehicle and the index [" + bodyCol + "] of this customization exists or isn't null."); }

        lastColTex = null;
    }

    private void ApplyRimCustomization(int rimIndex, int tovehicleIndex)
    {

        rims[rimIndex].unlocked = true;

        if (!PlayerPrefs.HasKey("VehicleRim" + rims[rimIndex].ID + menuVehicles[vehicleIndex].name))
            PlayerPrefs.SetInt("VehicleRim" + rims[rimIndex].ID + menuVehicles[vehicleIndex].name, 1);

        //Save as the vehicle's current rim
        PlayerPrefs.SetInt("CurrentRim" + menuVehicles[tovehicleIndex].name, rimIndex);

        try { menuVehicles[tovehicleIndex].VehicleRims.mainTexture = rims[rimIndex].visualUpgrade[tovehicleIndex].texture; }
        catch { Debug.LogError("You haven't properly configured rim customizations for this vehicle! Ensure you have assigned a material for your vehicle and the index [" + rimIndex + "] of this customization exists or isn't null."); }


        lastRimTex = null;
    }

    private void ApplyUpgradeCustomization()
    {
        switch (upgradeState)
        {
            case UpgradeState.Vehicle:
                if (menuVehicles[vehicleIndex].upgrade.vehicleLevel < menuVehicles[vehicleIndex].upgrade.maxLevel)
                {
                    menuVehicles[vehicleIndex].upgrade.vehicleLevel++;
                }
                break;

            case UpgradeState.Speed:
                if (menuVehicles[vehicleIndex].upgrade.speedLevel < menuVehicles[vehicleIndex].upgrade.vehicleLevel)
                {
                    menuVehicles[vehicleIndex].upgrade.speedLevel++;
                }
                break;

            case UpgradeState.Accel:
                if (menuVehicles[vehicleIndex].upgrade.accelerationLevel < menuVehicles[vehicleIndex].upgrade.vehicleLevel)
                {
                    menuVehicles[vehicleIndex].upgrade.accelerationLevel++;
                }
                break;

            case UpgradeState.Handle:
                if (menuVehicles[vehicleIndex].upgrade.handlingLevel < menuVehicles[vehicleIndex].upgrade.vehicleLevel)
                {
                    menuVehicles[vehicleIndex].upgrade.handlingLevel++;
                }
                break;

            case UpgradeState.Brake:
                if (menuVehicles[vehicleIndex].upgrade.brakingLevel < menuVehicles[vehicleIndex].upgrade.vehicleLevel)
                {
                    menuVehicles[vehicleIndex].upgrade.brakingLevel++;
                }
                break;
        }

        //강화관련 부분 저장
        PlayerPrefs.SetInt("vehicleLevel" + menuVehicles[vehicleIndex].name, menuVehicles[vehicleIndex].upgrade.vehicleLevel);
        PlayerPrefs.SetInt("maxLevel" + menuVehicles[vehicleIndex].name, menuVehicles[vehicleIndex].upgrade.maxLevel);
        PlayerPrefs.SetInt("speedLevel" + menuVehicles[vehicleIndex].name, menuVehicles[vehicleIndex].upgrade.speedLevel);
        PlayerPrefs.SetInt("accelLevel" + menuVehicles[vehicleIndex].name, menuVehicles[vehicleIndex].upgrade.accelerationLevel);
        PlayerPrefs.SetInt("handleLevel" + menuVehicles[vehicleIndex].name, menuVehicles[vehicleIndex].upgrade.handlingLevel);
        PlayerPrefs.SetInt("brakeLevel" + menuVehicles[vehicleIndex].name, menuVehicles[vehicleIndex].upgrade.brakingLevel);
        PlayerPrefs.SetInt("levelupValue" + menuVehicles[vehicleIndex].name, menuVehicles[vehicleIndex].upgrade.levelupValue);

        upgradeState = UpgradeState.None;

        UpdateUI();
    }

    /// <summary>
    /// Loads important values such as currency & preferences
    /// </summary>
    private void LoadValues()
    {
        PlayerData.LoadCurrency();

        //Last selected vehicle
        if (PlayerPrefs.HasKey("SelectedVehicle")) vehicleIndex = PlayerPrefs.GetInt("SelectedVehicle");

        //Master Vol
        if (masterVolume) masterVolume.value = (PlayerPrefs.HasKey("MasterVolume")) ? PlayerPrefs.GetFloat("MasterVolume") : 1;

        //Graphic Level
        if (PlayerPrefs.HasKey("GraphicLevel")) SetGraphicsQuality(PlayerPrefs.GetInt("GraphicLevel"));

        //Player Name
        if (PlayerPrefs.HasKey("PlayerName")) { if (playerName) playerName.text = PlayerPrefs.GetString("PlayerName"); }

        //Toggles
        if (!PlayerPrefs.HasKey("ButtonAcceleration"))
            PlayerPrefs.SetString("ButtonAcceleration", "True"); //초기값
        if (mobileAutoAcceleration) mobileAutoAcceleration.isOn = PlayerPrefs.GetString("AutoAcceleration") == "True";
        if (mobileButtonAcceleration) mobileButtonAcceleration.isOn = PlayerPrefs.GetString("ButtonAcceleration") == "True";
        if (mobileTouchSteer) mobileTouchSteer.isOn = PlayerPrefs.GetString("MobileControlType") == "Touch";
        if (mobileGyroSteer) mobileGyroSteer.isOn = PlayerPrefs.GetString("MobileControlType") == "Gyro";

        //Other important stuff
        CheckForUnlockedVehiclesAndTracks();
        LoadCustomizations();

    }

    private void LoadCustomizations()
    {
        for (int i = 0; i < menuVehicles.Length; i++)
        {
            if (PlayerPrefs.HasKey("CurrentBodyColor" + menuVehicles[i].name))
            {
                ApplyColorCustomization(PlayerPrefs.GetInt("CurrentBodyColor" + menuVehicles[i].name), i);
            }

            if (PlayerPrefs.HasKey("CurrentRim" + menuVehicles[i].name))
            {
                ApplyRimCustomization(PlayerPrefs.GetInt("CurrentRim" + menuVehicles[i].name), i);
            }
        }
    }

    private void CheckForUnlockedVehiclesAndTracks()
    {

        //Check for unlokced vehicles
        for (int i = 0; i < menuVehicles.Length; i++)
        {
            //First check if the vehicle is pre-unlocked
            if (menuVehicles[i].unlocked)
            {
                PlayerPrefs.SetInt(menuVehicles[i].name, 1);
            }

            if (PlayerPrefs.GetInt(menuVehicles[i].name) == 1)
            {
                menuVehicles[i].unlocked = true;
            }
            else
            {
                menuVehicles[i].unlocked = false;
            }
        }

        //Check for unlokced tracks
        for (int i = 0; i < menuTracks.Length; i++)
        {
            //First check if the track is pre-unlocked
            if (menuTracks[i].unlocked)
            {
                PlayerPrefs.SetInt(menuTracks[i].name, 1);
            }

            if (PlayerPrefs.GetInt(menuTracks[i].name) == 1)
            {
                menuTracks[i].unlocked = true;
            }
            else
            {
                menuTracks[i].unlocked = false;
            }
        }
    }

    private void CheckForUnlockedCustomizations()
    {
        for (int i = 0; i < bodyColors.Length; i++)
        {
            if (PlayerPrefs.GetInt("BodyColor" + bodyColors[i].ID + menuVehicles[vehicleIndex].name) == 1)
            {
                bodyColors[i].unlocked = true;
            }
            else
            {
                bodyColors[i].unlocked = false;
            }
        }

        for (int i = 0; i < rims.Length; i++)
        {
            if (PlayerPrefs.GetInt("VehicleRim" + rims[i].ID + menuVehicles[vehicleIndex].name) == 1)
            {
                rims[i].unlocked = true;
            }
            else
            {
                rims[i].unlocked = false;
            }
        }
    }

    private void RevertCustomizationChanges()
    {
		if (lastColTex && menuVehicles[vehicleIndex].vehicleBody)
			menuVehicles[vehicleIndex].vehicleBody.SetTexture("_BaseMap", lastColTex);
			//menuVehicles[vehicleIndex].vehicleBody.mainTexture = lastColTex;
        if (lastRimTex && menuVehicles[vehicleIndex].VehicleRims)
			menuVehicles[vehicleIndex].VehicleRims.mainTexture = lastRimTex;

        incartCr = 0;
        bodyColPrice = 0;
        rimPrice = 0;
        upgradePrice = 0;
        selectedColorID = -1;
        selectedRimID = -1;
        selectedUpgradeID = -1;
        lastColTex = null;
        lastRimTex = null;
        upgradeState = UpgradeState.None;

        //for (int i = 0; i < bodyColors.Length; i++)
        //{
        //    bodyColors[i].unlocked = false;
        //}
    }

    private void CreatePromptPanel(string title, string prompt)
    {

        if (promptTitle) promptTitle.text = title;

        if (promptText) promptText.text = prompt;

        if (promptPanel) promptPanel.SetActive(true);
    }

    #region Button Functions
    public void NextArrow()
    {
        ButtonSFX();

        if (state == State.Main)
        {
            if (vehicleIndex < menuVehicles.Length - 1)
            {
                prevVehicleIndex = vehicleIndex;
                vehicleIndex++;
            }
            else
            {
                prevVehicleIndex = vehicleIndex;
                vehicleIndex = 0;
            }

            CycleVehicles();
        }

        if (state == State.TrackSelect)
        {
            if (trackIndex < menuTracks.Length - 1)
            {
                trackIndex++;
            }
            else
            {
                trackIndex = 0;
            }

            CycleTracks();
        }
    }

    public void PreviousArrow()
    {
        ButtonSFX();

        if (state == State.Main)
        {
            if (vehicleIndex > 0)
            {
                prevVehicleIndex = vehicleIndex;
                vehicleIndex--;
            }
            else
            {
                prevVehicleIndex = vehicleIndex;
                vehicleIndex = menuVehicles.Length - 1;
            }

            CycleVehicles();
        }

        if (state == State.TrackSelect)
        {
            if (trackIndex > 0)
            {
                trackIndex--;
            }
            else
            {
                trackIndex = menuTracks.Length - 1;
            }

            CycleTracks();
        }
    }

    public void Play()
    {
        state = State.Loading;

        UpdateUI();

        //Save all preferences
        PlayerPrefs.SetString("PlayerVehicle", menuVehicles[vehicleIndex].resourceName);
        PlayerPrefs.SetString("RaceType", menuTracks[trackIndex].raceType.ToString());
        PlayerPrefs.SetString("AiDifficulty", menuTracks[trackIndex].aiDifficulty.ToString());
        PlayerPrefs.SetInt("Opponents", menuTracks[trackIndex].aiCount);
        PlayerPrefs.SetInt("Laps", menuTracks[trackIndex].laps);
        PlayerPrefs.SetInt("RaceTime", (int)menuTracks[trackIndex].raceTime);

        AdMobBannerView ad = GameObject.FindObjectOfType<AdMobBannerView>();
		ad.DestroyAdBanner();

        StartCoroutine(LoadScene());
    }


    public void Buy()
    {
        ButtonSFX();

        //BUY VEHILCE
        if (state == State.Main)
        {
            if (PlayerData.currency >= menuVehicles[vehicleIndex].price)
            {
                if (accept)
                {
                    accept.onClick.RemoveAllListeners();
                    accept.onClick.AddListener(() => AcceptPrompt());
                }

                if (cancel)
                {
                    cancel.gameObject.SetActive(true);
                    cancel.onClick.RemoveAllListeners();
                    cancel.onClick.AddListener(() => ClosePromptPanel());
                }

                CreatePromptPanel("CONFIRM ACTION", "Do you really want to purchase this vehicle?");
            }
            else
            {
                if (accept)
                {
                    accept.onClick.RemoveAllListeners();
                    accept.onClick.AddListener(() => ClosePromptPanel());
                }

                if (cancel) cancel.gameObject.SetActive(false);

                CreatePromptPanel("NOT ENOUGH CURRENCY", "You do not have enough currency to buy this vehicle");
            }
        }

        //BUY TRACK
        if (state == State.TrackSelect)
        {
            if (PlayerData.currency >= menuTracks[trackIndex].price)
            {
                if (accept)
                {
                    accept.onClick.RemoveAllListeners();
                    accept.onClick.AddListener(() => AcceptPrompt());
                }

                if (cancel)
                {
                    cancel.gameObject.SetActive(true);
                    cancel.onClick.RemoveAllListeners();
                    cancel.onClick.AddListener(() => ClosePromptPanel());
                }

                CreatePromptPanel("CONFIRM ACTION", "Do you really want to purchase this track?");
            }
            else
            {
                if (accept)
                {
                    accept.onClick.RemoveAllListeners();
                    accept.onClick.AddListener(() => ClosePromptPanel());
                }

                if (cancel) cancel.gameObject.SetActive(false);

                CreatePromptPanel("NOT ENOUGH CURRENCY", "You do not have enough currency to buy this track");
            }
        }


        //BUY CUSTOMIZATION
        if (state == State.Tuning)
        {
            if (PlayerData.currency >= incartCr)
            {
                if (accept)
                {
                    accept.onClick.RemoveAllListeners();
                    accept.onClick.AddListener(() => AcceptPrompt());
                }

                if (cancel)
                {
                    cancel.gameObject.SetActive(true);
                    cancel.onClick.RemoveAllListeners();
                    cancel.onClick.AddListener(() => ClosePromptPanel());
                }

                CreatePromptPanel("CONFIRM ACTION", "Do you really want to make this purchase?");
            }
            else
            {
                if (accept)
                {
                    accept.onClick.RemoveAllListeners();
                    accept.onClick.AddListener(() => ClosePromptPanel());
                }

                if (cancel) cancel.gameObject.SetActive(false);

                CreatePromptPanel("NOT ENOUGH CURRENCY", "You do not have enough currency to make this purchase");
            }
        }
    }

    public void UpgradeValueSelect(int value)
    {
        ButtonSFX();

        upgradeState = (UpgradeState)value;

        if (upgradeState == UpgradeState.Vehicle)
            upgradePrice = 1000;
        else
            upgradePrice = 500;

        UpdateUI();
    }    

    public void TuningSelect()
    {
        ButtonSFX();

        state = State.Tuning;

        UpdateUI();
    }

    public void ModeSelect()
    {
        ButtonSFX();

        if (menuVehicles[vehicleIndex].unlocked) PlayerPrefs.SetInt("SelectedVehicle", vehicleIndex);
        TrackSelect();

        UpdateUI();
    }

    public void TrackSelect()
    {
        ButtonSFX();

        if (menuVehicles[vehicleIndex].unlocked) PlayerPrefs.SetInt("SelectedVehicle", vehicleIndex);
        state = State.TrackSelect;

        UpdateUI();
    }

    //public void Customize()
    //{
    //    ButtonSFX();

    //    state = State.Customize_Upgrade;

    //    CheckForUnlockedCustomizations();

    //    UpdateUI();
    //}

    public void Settings()
    {
        ButtonSFX();

        if (state != State.Settings) previousState = state;

        state = state != State.Settings ? State.Settings : previousState;

        UpdateUI();
    }

    public void SetCustomState(int customstate)
    {
        ButtonSFX();

        this.customState = (CustomState)customstate;

        UpdateUI();
    }


    public void SetGraphicsQuality(int level)
    {
        QualitySettings.SetQualityLevel(level, applyExpensiveGraphicChanges);

        PlayerPrefs.SetInt("GraphicLevel", level);
    }

    private void GetGrahicLevelFromDropdown()
    {
        SetGraphicsQuality(graphicLevel.value);
    }

    private void SetMasterVolFromSlider()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume.value);

        if (SoundManager.instance)
            SoundManager.instance.SetVolume();
    }

    private void SetPlayerNameFromInputField()
    {
        PlayerPrefs.SetString("PlayerName", playerName.text);
    }

    public void ToggleTouchControl()
    {
        if (mobileTouchSteer.isOn)
        {
            mobileGyroSteer.isOn = false;
            PlayerPrefs.SetString("MobileControlType", "Touch");
        }
        else
        {
            mobileGyroSteer.isOn = true;
            ToggleGyroControl();
        }
    }

    public void ToggleGyroControl()
    {
        if (mobileGyroSteer.isOn)
        {
            mobileTouchSteer.isOn = false;
            PlayerPrefs.SetString("MobileControlType", "Gyro");
        }
        else
        {
            mobileTouchSteer.isOn = true;
            ToggleTouchControl();
        }
    }

    public void ToggleAutoAccel()
    {
        string isOn = mobileAutoAcceleration.isOn ? "True" : "False";
        PlayerPrefs.SetString("AutoAcceleration", isOn);
    }

    public void ToggleButtonAccel()
    {
        string isOn = mobileButtonAcceleration.isOn ? "True" : "False";
        PlayerPrefs.SetString("ButtonAcceleration", isOn);
    }

    public void ChooseVehicle()
    {
        PlayerPrefs.SetInt("SelectedVehicle", vehicleIndex);
        Back();
    }

    public void AdjustRaceType(int val)
    {
        //CanPlayRaceTypes 배열에 값이 없다면 모든 레이스 타입을 사용
        if (menuTracks[trackIndex].CanPlayRaceTypes == null || menuTracks[trackIndex].CanPlayRaceTypes.Length == 0)
        {
            int raceTypeValue = (int)menuTracks[trackIndex].raceType + val;
            raceTypeValue = Mathf.Clamp(raceTypeValue, 0, Enum.GetNames(typeof(RaceManager.RaceType)).Length - 1);
            menuTracks[trackIndex].raceType = (RaceManager.RaceType)raceTypeValue;
        }
        //CanPlayRaceTypes 배열에 값이 존재 할 경우 해당 배열의 레이스 타입만을 사용
        else
        {
            for (int i = 0; i < menuTracks[trackIndex].CanPlayRaceTypes.Length; i++)
            {
                if (menuTracks[trackIndex].raceType == menuTracks[trackIndex].CanPlayRaceTypes[i])
                {
                    int raceTypeValue = i + val;
                    raceTypeValue = Mathf.Clamp(raceTypeValue, 0, menuTracks[trackIndex].CanPlayRaceTypes.Length - 1);
                    menuTracks[trackIndex].raceType = menuTracks[trackIndex].CanPlayRaceTypes[raceTypeValue];
                }
            }
        }

        ///기존 코드
        /*
        raceTypeIndex += val;
        raceTypeIndex = Mathf.Clamp(raceTypeIndex, 1, Enum.GetNames(typeof(RaceManager.RaceType)).Length);

        switch (raceTypeIndex)
        {

            case 1:
                menuTracks[trackIndex].raceType = RaceManager.RaceType.Circuit;
                break;

            case 2:
                menuTracks[trackIndex].raceType = RaceManager.RaceType.LapKnockout;
                break;

            case 3:
                menuTracks[trackIndex].raceType = RaceManager.RaceType.TimeTrial;
                break;

            case 4:
                menuTracks[trackIndex].raceType = RaceManager.RaceType.SpeedTrap;
                break;

            case 5:
                menuTracks[trackIndex].raceType = RaceManager.RaceType.Checkpoints;
                break;

            case 6:
                menuTracks[trackIndex].raceType = RaceManager.RaceType.Elimination;
                break;

            case 7:
                menuTracks[trackIndex].raceType = RaceManager.RaceType.Drift;
                break;
        }
        */
        UpdateUI();
    }

    public void AdjustLaps(int val)
    {
        menuTracks[trackIndex].laps += val;
        menuTracks[trackIndex].laps = Mathf.Clamp(menuTracks[trackIndex].laps, 1, 1000);

        UpdateUI();
    }

    public void AdjustAiCount(int val)
    {
        menuTracks[trackIndex].aiCount += val;
        menuTracks[trackIndex].aiCount = Mathf.Clamp(menuTracks[trackIndex].aiCount, 0, maxOpponents);

        UpdateUI();
    }

    public void AdjustAiDifficulty(int val)
    {

        aiDiffIndex += val;
        aiDiffIndex = Mathf.Clamp(aiDiffIndex, 1, 4);

        switch (aiDiffIndex)
        {

            case 1:
                menuTracks[trackIndex].aiDifficulty = OpponentControl.AiDifficulty.Custom;
                break;

            case 2:
                menuTracks[trackIndex].aiDifficulty = OpponentControl.AiDifficulty.Easy;
                break;

            case 3:
                menuTracks[trackIndex].aiDifficulty = OpponentControl.AiDifficulty.Meduim;
                break;

            case 4:
                menuTracks[trackIndex].aiDifficulty = OpponentControl.AiDifficulty.Hard;
                break;
        }

        UpdateUI();
    }

    public void AdjustRaceTime(int val)
    {
        menuTracks[trackIndex].raceTime = (menuTracks[trackIndex].raceTime == RaceManager.RaceTime.Day) ? RaceManager.RaceTime.Night : RaceManager.RaceTime.Day;

        UpdateUI();
    }

    public void SelectColor(int c)
    {
        ButtonSFX();

        if (!menuVehicles[vehicleIndex].vehicleBody) return;

		if (!lastColTex)
		{
			lastColTex = menuVehicles[vehicleIndex].vehicleBody.GetTexture("_BaseMap");
			//lastColTex = menuVehicles[vehicleIndex].vehicleBody.mainTexture;
		}

        for (int i = 0; i < bodyColors.Length; i++)
        {
            if (c == bodyColors[i].ID)
            {
                selectedColorID = i;
                bodyColPrice = !bodyColors[selectedColorID].unlocked ? bodyColors[selectedColorID].price : 0;

                try
				{
					menuVehicles[vehicleIndex].vehicleBody.SetTexture("_BaseMap", bodyColors[i].visualUpgrade[vehicleIndex].texture);
					//menuVehicles[vehicleIndex].vehicleBody.mainTexture = bodyColors[i].visualUpgrade[vehicleIndex].texture;
				}
                catch { Debug.Log("You haven't properly configured color customizations for this vehicle! Ensure you have assigned a material for your vehicle and the index [" + i + "] of this customization exists or isn't null."); }
            }
        }

        UpdateUI();
    }

    public void SelectRim(int r)
    {
        ButtonSFX();

        if (!menuVehicles[vehicleIndex].VehicleRims) return;

        if (!lastRimTex) lastRimTex = menuVehicles[vehicleIndex].VehicleRims.mainTexture;

        for (int i = 0; i < rims.Length; i++)
        {
            if (r == rims[i].ID)
            {
                selectedRimID = i;
                rimPrice = !rims[selectedRimID].unlocked ? rims[selectedRimID].price : 0;

                try { menuVehicles[vehicleIndex].VehicleRims.mainTexture = rims[i].visualUpgrade[vehicleIndex].texture; }
                catch { Debug.Log("You haven't properly configured rim customizations for this vehicle! Ensure you have assigned a material for your vehicle and the index [" + i + "] of this customization exists or isn't null."); }
            }
        }

        UpdateUI();
    }

    public void ApplyCustomizationChanges()
    {
        if (selectedColorID >= 0) ApplyColorCustomization(selectedColorID, vehicleIndex);

        if (selectedRimID >= 0) ApplyRimCustomization(selectedRimID, vehicleIndex);

        Back();
    }

    public void ApplyColor()
    {
        ButtonSFX();

        if (selectedColorID >= 0) ApplyColorCustomization(selectedColorID, vehicleIndex);

        Back();
    }

    public void AcceptPrompt()
    {
        switch (state)
        {
            case State.Main:
                PlayerData.DeductCurrency(menuVehicles[vehicleIndex].price);

                menuVehicles[vehicleIndex].unlocked = true;
                PlayerPrefs.SetInt(menuVehicles[vehicleIndex].name, 1);
                break;

            case State.TrackSelect:
                PlayerData.DeductCurrency(menuTracks[trackIndex].price);

                menuTracks[trackIndex].unlocked = true;
                PlayerPrefs.SetInt(menuTracks[trackIndex].name, 1);
                break;

            case State.Tuning:
                PlayerData.DeductCurrency(incartCr);

                switch (customState)
                {
                    case CustomState.Color:
                        ApplyColorCustomization(selectedColorID, vehicleIndex);
                        break;
                    case CustomState.Upgrade:
                        ApplyUpgradeCustomization();
                        break;

                }

                //if (selectedRimID >= 0) ApplyRimCustomization(selectedRimID, vehicleIndex);

                //Back();
                break;
        }

        UpdateUI();
        ClosePromptPanel();
    }

    public void ClosePromptPanel()
    {
        if (promptPanel) promptPanel.SetActive(false);

        RevertCustomizationChanges();

        UpdateUI();
    }

    public void Back()
    {
        ButtonSFX();

        switch (state)
        {
            case State.Main:
                Application.Quit();
                break;

            case State.Tuning:
                RevertCustomizationChanges();
                state = State.Main;
                CycleVehicles();
                break;

            case State.TrackSelect:
                state = State.Main;
                break;

            //case State.Customize_Upgrade:
            //    RevertCustomizationChanges();
            //    state = State.Tuning;
            //    break;

            case State.Settings:
                state = State.Main;
                break;
        }

        UpdateUI();
    }
    #endregion

    IEnumerator LoadScene()
    {
        async = SceneManager.LoadSceneAsync(menuTracks[trackIndex].sceneName);

        while (!async.isDone)
        {
            if (loadingBar) loadingBar.fillAmount = async.progress;

            yield return null;
        }
    }

    void ButtonSFX()
    {
        if (SoundManager.instance) SoundManager.instance.PlaySound("Button", true);
    }

}

