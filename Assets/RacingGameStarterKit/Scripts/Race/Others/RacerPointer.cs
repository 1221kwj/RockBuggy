//racerpointer.cs is used to display a minimap pointer above a racer

using UnityEngine;

namespace RGSK
{
    public class RacerPointer : MonoBehaviour
    {
        public  Transform   target;
        public  float       height = -50.0f;
        private float       wantedHeight;

        void Start()
        {
            wantedHeight = target.position.y + height;
        }

        void Update()
        {
            if (!target) return;

            //follow the racer
            transform.position = new Vector3(target.position.x, target.position.y + height, target.position.z);

            //Rotate in the direction of the racer
            Quaternion rot = transform.rotation;
            rot = target.rotation;
            rot.x = 0;
            rot.z = 0;
            transform.rotation = rot;
        }
    }
}
