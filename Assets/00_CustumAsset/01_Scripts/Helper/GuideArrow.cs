using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GuideArrow : MonoBehaviour
{
	private float orgAlpha;

	private Image myImg;
	private Color color;

	private bool active;

	// Start is called before the first frame update
	private void Awake()
	{
		myImg = GetComponent<Image>();
		color = myImg.color;
		orgAlpha = myImg.color.a;
	}

	void Start()
    {
		myImg = GetComponent<Image>();
		color = myImg.color;
		orgAlpha = myImg.color.a;

		StartCoroutine(Arrow());
    }

	IEnumerator Arrow()
	{
		if (color == null)
			color = myImg.color;

		while (true)
		{
			color.a = Mathf.Lerp(color.a, 0.0f, Time.deltaTime * 10.0f);

			if (color.a < 0.01f)
				color.a = orgAlpha;

			myImg.color = color;

			yield return new WaitForSeconds(0.02f);
		}
	}

	private void OnEnable()
	{
		StartCoroutine(Arrow());
	}
}
