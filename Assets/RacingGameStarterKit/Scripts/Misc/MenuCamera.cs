using UnityEngine;

namespace RGSK
{
    public class MenuCamera : MonoBehaviour
    {

        public Transform target;        

        [Header("Settings")]
        public float zoomMultiplier = 8.0f;
        public float xSpeed = 120.0f;
        public float ySpeed = 120.0f;
        public float yMinLimit = -20f;
        public float yMaxLimit = 80f;
        public float distanceMin = .5f;
        public float distanceMax = 15f;
        private float newDistance;
        private bool orbit;
        private bool allowTouchOrbit;
        private Touch touch;
        [HideInInspector]
        public bool canOrbit;

        [Header("Current Values")]
        public float distance = 5.0f;
        public float x = 0.0f;
        public float y = 0.0f;
        public float velX;
        public float velY;

        private float targetY = -0.12f;
        private float lastMultiTouchLength;

        void Start()
        {
            newDistance = distance;
            canOrbit = true;

            //preset values
            SetValues(-4f, 18.0f);

            //automatically enable touch orbit on mobile platforms
#if (UNITY_ANDROID || UNITY_IOS || UNITY_WP_8) && !UNITY_EDITOR
            allowTouchOrbit = true;
#else
            allowTouchOrbit = false;
#endif

        }


        void Update()
        {
            orbit = (!allowTouchOrbit) ? Input.GetButton("Fire2") : Input.touchCount == 1;

            orbit &= !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject((allowTouchOrbit) ? 0 : -1);

            //UI크기에 따른 위치 조정
            if (CustomMenuManager.Instance != null)
            {
                if(CustomMenuManager.Instance.state == CustomMenuManager.State.Tuning)
                {
                    targetY = Mathf.Lerp(targetY, -1.0f, Time.deltaTime);
                    target.transform.localPosition = new Vector3(0, targetY, 0);
                }
                else
                {
                    targetY = Mathf.Lerp(targetY, -0.12f, Time.deltaTime);
                    target.transform.localPosition = new Vector3(0, targetY, 0);
                }
            }
        }

        void LateUpdate()
        {
            if (target)
            {
                if (canOrbit && orbit &&
                    ((CustomMenuManager.Instance != null)? (CustomMenuManager.Instance.state == CustomMenuManager.State.Main
                        || CustomMenuManager.Instance.state == CustomMenuManager.State.Tuning) : false)
                    )
                {

                    if (!allowTouchOrbit)
                    {
                        velX += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                        velY -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                    }
                    else
                    {
                        if (Input.GetTouch(0).phase == TouchPhase.Moved)
                        {
                            touch = Input.GetTouch(0);
                            velX += touch.deltaPosition.x * xSpeed * 0.002f;
                            velY -= touch.deltaPosition.y * ySpeed * 0.002f;
                        }
                    }
                }

                //Lerp values for smoooth rotation
                //y = Mathf.Lerp(y, velY, Time.deltaTime);
                //x = Mathf.Lerp(x, velX, Time.deltaTime);
                y = velY;
                x = velX;

                //Clamp the angles
                y = ClampAngle(y, yMinLimit, yMaxLimit);
                velY = ClampAngle(velY, yMinLimit, yMaxLimit);
                Quaternion rotation = Quaternion.Euler(y, x, 0);

                if (Input.touchCount < 2)
                {
                    //Calculate distance from mouse scroll wheel
                    newDistance = Mathf.Clamp(newDistance, distanceMin, distanceMax);
                    newDistance += Input.GetAxis("Mouse ScrollWheel") * -zoomMultiplier;
                    distance = Mathf.Lerp(distance, newDistance, Time.deltaTime);
                    lastMultiTouchLength = 0;
                }
                else
                {
                    newDistance = Mathf.Clamp(newDistance, distanceMin, distanceMax);
                    float newMultiTouchLenth = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                    if(lastMultiTouchLength != 0 && newMultiTouchLenth != 0) newDistance *= lastMultiTouchLength / newMultiTouchLenth;
                    lastMultiTouchLength = newMultiTouchLenth;                    
                    distance = Mathf.Lerp(distance, newDistance, Time.deltaTime);
                }

                Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
                Vector3 position = rotation * negDistance + target.position;
                //set the camera rot & pos
                transform.rotation = rotation;
                transform.position = position;
            }
        }

        public void MouseOverRectTransform(bool isOver)
        {
            canOrbit = isOver;
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }

        public void SetValues(float xVal, float yVal)
        {
            velX = xVal;
            velY = yVal;
        }
    }
}
