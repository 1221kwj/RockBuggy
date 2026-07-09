using RGSK;
using UnityEngine;

public class ReturnPointTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.GetComponentInParent<Statistics>().tag + " passed : " + this.gameObject.name);

		//if (other.GetComponentInParent<Statistics>().tag == "Player")
		
		if(other.transform.root.tag == "Player")
        {
            other.GetComponentInParent<ReturnToLastPoint>().LastCheckPoint = this.transform;
            this.enabled = false;
        }
    }
}
