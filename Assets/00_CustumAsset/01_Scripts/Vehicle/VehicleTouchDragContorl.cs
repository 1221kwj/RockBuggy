using UnityEngine;
using UnityEngine.UI;

public class VehicleTouchDragContorl : MonoBehaviour
{
    public float MaxTouchArea = 400.0f;
    Vector2 firstTouchPosition;
    Image img;
    // Start is called before the first frame update
    void Start()
    {
        img = GetComponentInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (MobileInputManager.Instance != null && Input.touchCount > 0 && !MobileInputManager.Instance.ButtonAccelContorl)
        {
            if (!img.enabled)
                img.enabled = true;
            if (firstTouchPosition == new Vector2(-1f, -1f))
            {
                firstTouchPosition = Input.GetTouch(0).position;
                img.GetComponent<RectTransform>().position = Input.GetTouch(0).position;
            }
            float diff = firstTouchPosition.y - Input.GetTouch(0).position.y;

			if (diff < 0)
                MobileInputManager.Instance.AccelButtonPressed = true;
            else if (diff > 0)
                MobileInputManager.Instance.BrakeButtonPressed = true;
            MobileInputManager.Instance.AccelBrakeValue = Mathf.Abs(diff / MaxTouchArea);
        }
        else
        {
            if (img.enabled)
                img.enabled = false;
            firstTouchPosition = new Vector2(-1f, -1f);
            MobileInputManager.Instance.AccelBrakeValue = 0;
        }
    }
}
