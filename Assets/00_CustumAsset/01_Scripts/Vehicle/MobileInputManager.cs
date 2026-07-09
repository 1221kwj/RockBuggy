using NWH.VehiclePhysics;
using RGSK;

using UnityEngine;

/// <summary>
/// Class for handling desktop user input via mouse and keyboard.
/// Avoid having two input managers active at the sime time (mobile and destop) as the last executed script will override the first one.
/// </summary>
[DisallowMultipleComponent]
public class MobileInputManager : MonoBehaviour
{
    /// <summary>
    /// Type of input user input.
    /// Standard - standard keyboard, joystick or gamepad input mapped through the input manager.
    /// Mouse - uses mouse position on screen to control throttle/braking and steering.
    /// MouseSteer - uses LMB / RMB for throttle and braking and mouse for steering.
    /// </summary>
    public enum InputType { Standard, Mouse, MouseSteer, Mobile_Touch, Mobile_Gyro}

    [Tooltip("Input type. " +
        "Standard - uses standard input manager for all the inputs. " +
        "Mouse - uses mouse position for steering and throttle. " +
        "MouseSteer - uses mouse position for steering, LMB and RMB for braking / throttle.")]
    public InputType inputType = InputType.Mobile_Touch;


    public enum VerticalInputType { Standard, ZeroToOne, Composite }

    [Tooltip("Vertical input type." +
        "Standard - uses vertical axis in range of [-1, 1] where -1 is maximum braking and 1 maximum accleration." +
        "ZeroToOne - uses vertical axis in range of [0, 1], 0 being maximum braking and 1 maximum acceleration." +
        "Composite - uses separate axes, 'Accelerator' and 'Brake' to set the vertical axis value. Still uses a single vartical axis value [-1, 1] " +
        "throughout the system so applying full brakes and gas simultaneously is not possible.")]
    public VerticalInputType verticalInputType = VerticalInputType.Composite;

    /// <summary>
    /// Set to null (none) if you want to use your own vehicle controller. If this is set to other than null current active vehicle according 
    /// to the assigned vehicle changer will be used instead of the assigned vehicle controller.
    /// </summary>
    [Tooltip("Set to null (none) if you want to use your own vehicle controller. If this is set to other than null current active vehicle according " +
        "to the assigned vehicle changer will be used instead of the assigned vehicle controller.")]
    //public VehicleChanger vehicleChanger;

    /// <summary>
    /// If you want to use this script with a single vehicle or want to set your own vehicle controller from script set vehicle changer field to null / none.
    /// </summary>
    //[Tooltip("If you want to use this script with a single vehicle or want to set your own vehicle controller from script set vehicle changer field to null / none.")]
    private VehicleController vehicleController;

    private float vertical = 0f;
    public float Vertical { get { return vertical; } }
    private float horizontal = 0f;
    public float Horizontal { get { return horizontal; } }

    // Mobile_Gyro Buttons
    private bool _AccelButtonPressed;
    private bool _BrakeButtonPressed;
    [HideInInspector]
    public bool AccelButtonPressed
    {
        get
        {
            return _AccelButtonPressed;
        }
        set
        {
            if (value)
            {
                _BrakeButtonPressed = false;
                _AccelButtonPressed = value;
            }
            else
                _AccelButtonPressed = value;
        }
    }
    [HideInInspector]
    public bool BrakeButtonPressed
    {
        get
        {

            return _BrakeButtonPressed;
        }
        set
        {
            if (value)
            {
                _AccelButtonPressed = false;
                _BrakeButtonPressed = value;
            }
            else
                _BrakeButtonPressed = value;
        }
    }

    ///<summary>
    /// Accel, Brake growth rate
    ///</summary>
    public float AccelBrakeRate = 3.0f;

    ///<summary>
    /// Accessible Accel, Brake value
    ///</summary>
    public float AccelBrakeValue;
    public float ButtonAccelBrakeValue = 0.01f;

    ///<summary>
    /// Accel gyro growth rate
    ///</summary>
    public float AccelGyroRate = 1.0f;    

    ///<summary>
    /// Accel Touch Region
    ///</summary>
    //public GameObject accelRegion;

    ///<summary>
    /// Brake Touch Region
    ///</summary>
    //public GameObject brakeRegion;

	private GameObject player;
    private bool _ButtonAccelControl;
    public bool ButtonAccelContorl { get { return _ButtonAccelControl; } }

    private static MobileInputManager _instance;
    public static MobileInputManager Instance
    {
        get
        {
            if (_instance != null) return _instance;
            else return null;
        }
    }

    /// <summary>
    /// Tries to get the button value through input manager, if not falls back to hardcoded default value.
    /// </summary>
    private bool TryGetButtonDown(string buttonName, KeyCode altKey)
    {
        try
        {
            return Input.GetButtonDown(buttonName);
        }
        catch
        {
#if UNITY_EDITOR
			Debug.LogWarning(buttonName + " input binding missing, falling back to default. Check Input section in manual for more info.");
#endif
			return Input.GetKeyDown(altKey);
        }
    }


    /// <summary>
    /// Tries to get the button value through input manager, if not falls back to hardcoded default value.
    /// </summary>
    private bool TryGetButton(string buttonName, KeyCode altKey)
    {
        try
        {
            return Input.GetButton(buttonName);
        }
        catch
        {
#if UNITY_EDITOR
			Debug.LogWarning(buttonName + " input binding missing, falling back to default. Check Input section in manual for more info.");
#endif
			return Input.GetKey(altKey);
        }
    }


    private void Start()
    {
		//vehicleController = GetComponent<VehicleController>();
		if (_instance == null) _instance = this;
		player = GameObject.FindWithTag("Player");
		vehicleController = player.GetComponent<VehicleController>();
		_ButtonAccelControl = (PlayerPrefs.GetString("ButtonAcceleration") == "True");
#if !UNITY_EDITOR
        //PlayerPrefs에서 자동차 조종 부분 불러오기
        if (!PlayerPrefs.HasKey("MobileControlType"))
        {
            PlayerPrefs.SetString("MobileControlType", "Gyro"); // 설정된게 없었다면 기본값
        }

        switch (PlayerPrefs.GetString("MobileControlType"))
        {
            case "Touch":
                inputType = InputType.Mobile_Touch;
                break;

            case "Gyro":
                inputType = InputType.Mobile_Gyro;
                break;
        }
#else
		inputType = InputType.Standard;
#endif
	}

	public void Initialize()
	{
		
	}

	public void RespawnButtonPressed()
    {
        if (vehicleController != null)
        {
			RaceManager.instance.RespawnRacer(player.transform, player.GetComponent<Statistics>().lastPassedNode, 0.1f);
        }
    }

    public static float GetTouchVertical()
    {
        float percent = Mathf.Clamp(Input.GetTouch(0).position.y / Screen.height, -1f, 1f);
        if (percent < 0.5f)
            return -(0.5f - percent) * 2.0f;
        return (percent - 0.5f) * 2.0f;
    }

    public static float GetTouchHorizontal()
    {
        float percent = Mathf.Clamp(Input.GetTouch(0).position.x / Screen.width, -1f, 1f);
        if (percent < 0.5f)
            return -(0.5f - percent) * 2.0f;
        return (percent - 0.5f) * 2.0f;
    }

    void Update()
    {
		//if (vehicleChanger != null)
		//	vehicleController = vehicleChanger.ActiveVehicleController;
		//else return;

		//if (RaceManager.instance.raceStarted == false) return;

        try
        {
            if (ButtonAccelContorl)
            {
                if (!AccelButtonPressed && !BrakeButtonPressed) vertical = 0f;
            }
            else vertical = 0f;
            horizontal = 0f;

            if (vehicleController == null) return;

            // Manual shift
            if (TryGetButtonDown("ShiftUp", KeyCode.R))
                vehicleController.input.ShiftUp = true;

            if (TryGetButtonDown("ShiftDown", KeyCode.F))
                vehicleController.input.ShiftDown = true;

            if (vehicleController.transmission.transmissionType == Transmission.TransmissionType.Manual)
            {
                try
                {
                    if (TryGetButtonDown("FirstGear", KeyCode.Alpha1))
                        vehicleController.transmission.ShiftInto(1);
                    else if (TryGetButtonDown("SecondGear", KeyCode.Alpha2))
                        vehicleController.transmission.ShiftInto(2);
                    else if (TryGetButtonDown("ThirdGear", KeyCode.Alpha3))
                        vehicleController.transmission.ShiftInto(3);
                    else if (TryGetButtonDown("FourthGear", KeyCode.Alpha4))
                        vehicleController.transmission.ShiftInto(4);
                    else if (TryGetButtonDown("FifthGear", KeyCode.Alpha5))
                        vehicleController.transmission.ShiftInto(5);
                    else if (TryGetButtonDown("SixthGear", KeyCode.Alpha6))
                        vehicleController.transmission.ShiftInto(6);
                    else if (TryGetButtonDown("SeventhGear", KeyCode.Alpha7))
                        vehicleController.transmission.ShiftInto(7);
                    else if (TryGetButtonDown("EightGear", KeyCode.Alpha8))
                        vehicleController.transmission.ShiftInto(8);
                    else if (TryGetButtonDown("NinthGear", KeyCode.Alpha9))
                        vehicleController.transmission.ShiftInto(9);
                    else if (TryGetButtonDown("Neutral", KeyCode.Alpha0))
                        vehicleController.transmission.ShiftInto(0);
                    else if (TryGetButtonDown("Reverse", KeyCode.Minus))
                        vehicleController.transmission.ShiftInto(-1);
                }
                catch
                {
#if UNITY_EDITOR
					Debug.LogWarning("Some of the gear changing inputs might not be assigned in the input manager. Direct gear shifting " +
                        "will not work.");
#endif
				}
            }

            // Horizontal axis
            if (inputType == InputType.Standard)
            {
				horizontal = Input.GetAxis("Horizontal");
            }
            else if (inputType == InputType.Mobile_Touch)
            {
                if (Input.touchCount > 0)
                    horizontal = Mathf.Clamp(GetTouchHorizontal(), -1f, 1f);
                else
                    horizontal = 0;
            }
            else if (inputType == InputType.Mobile_Gyro)
            {

				float gyroAngle = Assets.Scripts.GyroDevice.Get().eulerAngles.z / 360.0f;

				if (gyroAngle <= 0.4f && gyroAngle >= 0.0f)
					gyroAngle = gyroAngle * 100.0f / 25.0f;
				else if (gyroAngle <= 1.0f && gyroAngle >= 0.6f)
					gyroAngle = (gyroAngle - 1.0f) * 100.0f / 25.0f;
				else gyroAngle = 0;

				horizontal = Mathf.Clamp(-gyroAngle * AccelGyroRate, -1f, 1f);
			}
            else
            {
                horizontal = Mathf.Clamp(VehicleController.GetMouseHorizontal(), -1f, 1f);
            }

			if (RaceManager.instance.raceStarted == true)
				vehicleController.input.Horizontal = horizontal;

            // Vertical axis
            if (inputType == InputType.Standard)
            {
                if (verticalInputType == VerticalInputType.Standard)
                {
					 //return;
						vertical = Input.GetAxisRaw("Vertical");
                }
                else if (verticalInputType == VerticalInputType.ZeroToOne)
                {
                    vertical = (Mathf.Clamp01(Input.GetAxisRaw("Vertical")) - 0.5f) * 2f;
                }
                else if (verticalInputType == VerticalInputType.Composite)
                {
                    float accelerator = Mathf.Clamp01(Input.GetAxisRaw("Accelerator"));
                    float brake = Mathf.Clamp01(Input.GetAxisRaw("Brake"));

					if (RaceManager.instance.raceStarted == true)
						vertical = accelerator - brake;
                }
            }
            else if (inputType == InputType.Mouse)
            {
                vertical = Mathf.Clamp(VehicleController.GetMouseVertical(), -1f, 1f);
            }
            //===============================================================================//
            else if (inputType == InputType.Mobile_Gyro || inputType == InputType.Mobile_Touch)
            {
                if (AccelButtonPressed)
                {
                    if (RaceManager.instance.raceStarted == true)
                        vertical += ((!_ButtonAccelControl) ? AccelBrakeValue : ButtonAccelBrakeValue) * AccelBrakeRate;

					if (vertical > 1.0f) vertical = 1.0f;
                }
                else if (BrakeButtonPressed)
                {
					if (RaceManager.instance.raceStarted == true)
						vertical += -((!_ButtonAccelControl) ? AccelBrakeValue : ButtonAccelBrakeValue) * AccelBrakeRate;
					
                    if (vertical < -1.0f) vertical = -1.0f;
                }
            }
            //===============================================================================//
            else
            {
                if (Input.GetMouseButton(0))
                    vertical = 1f;
                else if (Input.GetMouseButton(1))
                    vertical = -1f;
            }

			if (RaceManager.instance.raceStarted == true)
				vehicleController.input.Vertical = vertical;

            // Engine start/stop
            if (TryGetButtonDown("EngineStartStop", KeyCode.E))
            {
                vehicleController.engine.Toggle();
            }

            // Handbrake
            try
            {
                vehicleController.input.Handbrake = Input.GetAxis("Handbrake");
            }
            catch
            {
#if UNITY_EDITOR
				Debug.LogWarning("Handbrake axis not set up, falling back to default (Space).");
#endif
                vehicleController.input.Handbrake = Input.GetKey(KeyCode.Space) ? 1f : 0f;
			}

            // Clutch
            if (!vehicleController.transmission.automaticClutch)
            {
                try
                {
                    vehicleController.input.Clutch = Input.GetAxis("Clutch");
                }
                catch
                {
#if UNITY_EDITOR
					Debug.LogError("Clutch is set to manual but the required axis 'Clutch' is not set. " +
                        "Please set the axis inside input manager to use this feature.");
#endif
					vehicleController.transmission.automaticClutch = true;
                }
            }

            // Lights
            //if (TryGetButtonDown("LeftBlinker", KeyCode.Z))
            //{
            //    vehicleController.input.leftBlinker = !vehicleController.input.leftBlinker;
            //    if (vehicleController.input.leftBlinker) vehicleController.input.rightBlinker = false;
            //}
            //if (TryGetButtonDown("RightBlinker", KeyCode.X))
            //{
            //    vehicleController.input.rightBlinker = !vehicleController.input.rightBlinker;
            //    if (vehicleController.input.rightBlinker) vehicleController.input.leftBlinker = false;
            //}
            //if (TryGetButtonDown("Lights", KeyCode.L)) vehicleController.input.lowBeamLights = !vehicleController.input.lowBeamLights;
            //if (TryGetButtonDown("FullBeamLights", KeyCode.K)) vehicleController.input.fullBeamLights = !vehicleController.input.fullBeamLights;
            //if (TryGetButtonDown("HazardLights", KeyCode.J))
            //{
            //    vehicleController.input.hazardLights = !vehicleController.input.hazardLights;
            //    vehicleController.input.leftBlinker = false;
            //    vehicleController.input.rightBlinker = false;
            //}

            // Horn
            vehicleController.input.horn = TryGetButton("Horn", KeyCode.H);

            // Raise trailer flag if trailer attach detach button pressed.
            if (TryGetButtonDown("TrailerAttachDetach", KeyCode.T))
                vehicleController.input.trailerAttachDetach = true;

            // Manual flip over
            if (vehicleController.flipOver.manual)
            {
                try
                {
                    // Set manual flip over flag to true if vehicle is flipped over, otherwise ignore
                    if (Input.GetButtonDown("FlipOver") && vehicleController.flipOver.flippedOver)
                        vehicleController.input.flipOver = true;
                }
                catch
                {
#if UNITY_EDITOR
					Debug.LogError("Flip over is set to manual but 'FlipOver' input binding is not set. Either disable manual flip over or set 'FlipOver' binding.");
#endif
				}
            }
        }
        catch (System.Exception e)
        {
#if UNITY_EDITOR
			Debug.LogWarning("One or more of the required inputs has not been set. Check NWH Vehicle Physics README for more info or add the binding inside Unity input manager.");
            Debug.LogWarning(e);
#endif
		}
    }
}

