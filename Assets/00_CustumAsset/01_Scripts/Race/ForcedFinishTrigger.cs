using RGSK;
using UnityEngine;

public class ForcedFinishTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Statistics>().tag == "Player")
        {
            if (other.GetComponentInParent<Statistics>().lap == RaceManager.instance.totalLaps)
            {
                other.GetComponentInParent<Statistics>().NewLap();
            }
        }
    }
}
