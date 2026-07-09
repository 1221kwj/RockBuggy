using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuMaxRestoreButtonContorl : MonoBehaviour
{
    public int MENUWIDTH = 342; // MainMenu Width = 342;
    int CUTLINE = 5;
    float HIDESPEED = 50.0f;
    RectTransform MainMenuRoot;
    static bool _MenuHide = false;
    public static bool MenuHide
    {
        get { return _MenuHide; }
    }
    Text ButtonText;

    // Start is called before the first frame update
    void Start()
    {
        MainMenuRoot = this.transform.parent.GetComponent<RectTransform>();
        if (MainMenuRoot.anchoredPosition.x < 0) _MenuHide = true;

        ButtonText = GetComponentInChildren<Text>();
    }

    private void OnDisable()
    {
        _MenuHide = false;
    }

    public void MaxRestoreButtonDown()
    {
        StartCoroutine(HiddingMenu());
    }

    IEnumerator HiddingMenu()
    {
        if (_MenuHide)
        {
            ButtonText.text = "<<";
            while (MainMenuRoot.anchoredPosition.x < 0)
            {
                MainMenuRoot.anchoredPosition += new Vector2(Mathf.Abs(MainMenuRoot.anchoredPosition.x) / MENUWIDTH * HIDESPEED, 0);
                if (MainMenuRoot.anchoredPosition.x >= -CUTLINE)
                    MainMenuRoot.anchoredPosition = new Vector2(0, MainMenuRoot.anchoredPosition.y);
                yield return new WaitForSeconds(0);
            }
            _MenuHide = !_MenuHide;
        }
        else
        {
            ButtonText.text = ">>";
            while (MainMenuRoot.anchoredPosition.x > -MENUWIDTH)
            {
                MainMenuRoot.anchoredPosition
                    -= new Vector2((MENUWIDTH - Mathf.Abs(MainMenuRoot.anchoredPosition.x)) / MENUWIDTH * HIDESPEED, 0);
                if (MainMenuRoot.anchoredPosition.x <= -MENUWIDTH + CUTLINE)
                    MainMenuRoot.anchoredPosition = new Vector2(-MENUWIDTH, MainMenuRoot.anchoredPosition.y);
                yield return new WaitForSeconds(0);
            }
            _MenuHide = !_MenuHide;
        }
    }


}
