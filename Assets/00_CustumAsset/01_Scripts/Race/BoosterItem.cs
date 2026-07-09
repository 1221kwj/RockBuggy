using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterItem : MonoBehaviour
{
    // Start is called before the first frame update

    private Transform trans;

    [Range(2.0f, 15.0f)] public float boosterSpeed = 100.0f;

    public float BoosterSpeed
    {
        get { return boosterSpeed; }
    }

    void Start()
    {
        trans   = this.transform;
    }

    // Update is called once per frame
    void Update()
    {
        trans.Rotate(new Vector3(0, 1, 0), Time.deltaTime * 100.0f, Space.World);
    }
}
