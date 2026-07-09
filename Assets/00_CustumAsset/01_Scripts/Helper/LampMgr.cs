using System.Collections.Generic;
using UnityEngine;

using RGSK;

public class LampMgr : MonoBehaviour
{
	[SerializeField] private List<Light> lampLights;
	public void Initialize()
	{
		if (lampLights.Count > 0)
		{
			if (RaceManager.instance.raceTime == RaceManager.RaceTime.Day)
			{
				foreach (Light l in lampLights)
					if (l != null) l.enabled = false;
			}
			else
			{
				foreach (Light l in lampLights)
					if (l != null) l.enabled = true;
			}
		}
	}
}
