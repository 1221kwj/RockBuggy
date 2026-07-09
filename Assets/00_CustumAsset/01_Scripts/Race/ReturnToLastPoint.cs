using NWH.VehiclePhysics;
using System.Collections;
using UnityEngine;

public class ReturnToLastPoint : MonoBehaviour
{
    public float CutLine = -20.0f;

    [HideInInspector]
    public Transform LastCheckPoint;
    VehicleController vController;
    // Start is called before the first frame update
    void Start()
    {
        vController = GetComponent<VehicleController>();
    }

    public void GoToLastCheckPoint(float ignoreCollisionTime)
    {
        StartCoroutine(Respawn(ignoreCollisionTime));
    }

    IEnumerator Respawn(float ignoreCollisionTime)
    {
        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        transform.position = new Vector3(LastCheckPoint.position.x, LastCheckPoint.position.y + 2.5f, LastCheckPoint.position.z);
        transform.rotation = Quaternion.LookRotation(LastCheckPoint.forward);
        transform.rotation = LastCheckPoint.rotation;

        yield return new WaitForSeconds(ignoreCollisionTime);
    }

    public void GoToLastCheckPoint()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        if (vController) vController.Active = false;
        transform.position = LastCheckPoint.position;
        transform.rotation = LastCheckPoint.rotation;
        GetComponent<Rigidbody>().isKinematic = false;
        if (vController) vController.Active = true;
    }

    void Update()
    {
        if (transform.position.y < CutLine)
        {
            GoToLastCheckPoint(0.1f);
        }
    }
}
