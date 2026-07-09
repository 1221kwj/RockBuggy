using System.Collections;
using UnityEngine;

public class GuideHighlight : MonoBehaviour
{
	private Transform trans;
	private Vector3 orgScale = new Vector3(1.0f, 1.0f, 1.0f);
	private Vector3 limitScale = new Vector3(1.2f, 1.2f, 1.2f);
	private Vector3 targetScale = Vector3.zero;

	private float orgScaleScala;
	private float limitScaleScala;

	private int count;

	// Start is called before the first frame update
	void Start()
    {
		trans = this.GetComponent<Transform>();

		orgScaleScala	= orgScale.magnitude;
		limitScaleScala = limitScale.magnitude;
		targetScale		= limitScale;

		count = 10000;

		StartCoroutine(HightlightLerp());
	}

    // Update is called once per frame
    void Update()
    {
        if (this.isActiveAndEnabled)
			StartCoroutine(HightlightLerp());
	}

	public IEnumerator HightlightLerp()
	{
		if (trans == null)
			trans = this.GetComponent<Transform>();

		while (count >= 0)
		{
			float mag = trans.localScale.magnitude;

			float subCurLimit = Mathf.Abs(mag - limitScaleScala);
			float subCurOrg = Mathf.Abs(mag - orgScaleScala);

			if (subCurLimit < 0.01f)
				targetScale = orgScale;
			else if (subCurOrg < 0.01f)
				targetScale = limitScale;

			trans.localScale = Vector3.Lerp
			(
				trans.localScale,
				targetScale,
				Time.deltaTime * 10.0f
			);

			count--;

			yield return null;
		}

		yield return null;
	}
}
