using UnityEngine;

namespace RGSK
{

    /// <summary>
    /// MinimapFollowTarget is attached to your MinimapCamera to follow the player
    /// </summary>
    public class MinimapFollowTarget : MonoBehaviour
    {
		public enum MinimapCamType { OnlyPos, PosRot };

        public Transform    target;
        public float        height      = 100.0f;
        public float        distance    = 20.0f;

		public MinimapCamType miniCamType;

        void Update()
        {
            //find the target
            if (!target && GameObject.FindGameObjectWithTag("PlayerPointer"))
                target = GameObject.FindGameObjectWithTag("PlayerPointer").transform;
		}

        void LateUpdate()
        {
            if (!target) return;

			MinimapCamMove();
		}

		private void MinimapCamMove()
		{
			switch (miniCamType)
			{
				case MinimapCamType.OnlyPos:
				{
					transform.position = new Vector3(target.position.x, height, target.position.z - distance);

					break;
				}
				case MinimapCamType.PosRot:
				{
					Vector3 pos = transform.position;

					pos = target.position - target.forward * distance;
					pos.y = height;

					transform.position = pos;

					transform.LookAt(target.position);

					break;
				}
			}
		}
	}
}
