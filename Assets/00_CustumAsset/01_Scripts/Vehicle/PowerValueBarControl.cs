using UnityEngine;
using UnityEngine.UI;

public class PowerValueBarControl : MonoBehaviour
{
    public bool IsBackwardPower;
    Image img;
    MobileInputManager mim;
    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<Image>();
        mim = FindObjectOfType<MobileInputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsBackwardPower && mim.Vertical >= 0)
        {
            img.fillAmount = mim.Vertical;
        }
        else if (IsBackwardPower && mim.Vertical >= 0)
            img.fillAmount = 0;

        if (IsBackwardPower && mim.Vertical <= 0)
        {
            img.fillAmount = -mim.Vertical;
        }
        else if (!IsBackwardPower && mim.Vertical <= 0)
            img.fillAmount = 0;
    }
}