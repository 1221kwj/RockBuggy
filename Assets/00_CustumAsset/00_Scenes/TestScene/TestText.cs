using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using RGSK;
using NWH.VehiclePhysics;

public class TestText : MonoBehaviour
{
	private Text text;
	private GameObject player;

	private float time;
	private float velocity;

	private bool bLoadPlayerObject;
	private bool bCheckStart;
	private bool bCheckFinish;

    // Start is called before the first frame update
    void Start()
    {
		text = GetComponent<Text>();
		bCheckStart = false;
		time = 0.0f;
		velocity = 0.0f;

		bLoadPlayerObject = false;
		bCheckFinish = false;

		LoadPlayer();
	}

    // Update is called once per frame
    void Update()
    {
		if (bCheckStart == false && Input.GetKeyDown(KeyCode.W))
			bCheckStart = true;

		if (bCheckStart == true)
		{
			if (bLoadPlayerObject == false)
				LoadPlayer();

			//velocity = player.GetComponent<VehicleController>().SpeedKPH;

			if (RaceUI.instance != null)
			{
				string str = RaceUI.instance.vehicleUI.currentSpeed.text;

				str = str.Remove(str.IndexOf(" "));
				velocity = float.Parse(str);
			}

			if (velocity <= 100.0f && bCheckFinish == false)
			{
				time += Time.deltaTime;
				text.text = time.ToString("0.00") + "  " + velocity.ToString("0.00");
			}
			else
			{
				bCheckFinish = true;
			}
		}
    }

	public void Restart()
	{
		bCheckStart = false;
		bCheckFinish = false;
		time = 0.0f;
	}

	void LoadPlayer()
	{
		if (bLoadPlayerObject == true) return;

		if (RaceManager.instance != null)
		{
			player = RaceManager.instance.currentPlayer;
			bLoadPlayerObject = true;
		}
	}
}
