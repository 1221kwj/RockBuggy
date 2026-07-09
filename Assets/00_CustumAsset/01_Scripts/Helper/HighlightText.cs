using System.Collections;
using UnityEngine;

public class HighlightText : MonoBehaviour
{
	private Transform trans;
	private Vector3 orgScale = new Vector3(1.0f, 1.0f, 1.0f);
	private Vector3 limitScale = new Vector3(1.5f, 1.5f, 1.5f);
	private Vector3 targetScale = Vector3.zero;

	private bool bHighlight = true;

	private float orgScaleScala;
	private float limitScaleScala;

	public bool Hightlight
	{
		get { return bHighlight; }
		set { bHighlight = value; }
	}

	private void Awake()
	{
		bHighlight		= true;
		bHighlight		= true;
		trans			= this.GetComponent<Transform>();

		orgScaleScala	= orgScale.magnitude;
		limitScaleScala = limitScale.magnitude;
		targetScale		= limitScale;
	}

	public IEnumerator HightlightTextLerp()
	{
		if (trans == null)
			trans = this.GetComponent<Transform>();

		while (bHighlight == true)
		{
			float mag = trans.localScale.magnitude;

			float subCurLimit = Mathf.Abs(mag - limitScaleScala);
			float subCurOrg = Mathf.Abs(mag - orgScaleScala);

			if (subCurLimit < 0.01f)	targetScale = orgScale;
			else if (subCurOrg < 0.01f) targetScale = limitScale;

			trans.localScale = Vector3.Lerp
			(
				trans.localScale,
				targetScale,
				Time.deltaTime * 10.0f
			);

			yield return null;
		}

		trans.localScale = limitScale;
		yield break;
	}
}
