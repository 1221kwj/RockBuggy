using UnityEngine;

namespace Assets.Scripts
{
    public static class GyroDevice
    {
        private static bool gyro_Initialized = false;
        private static Vector3 eulerAngle = Vector3.zero;

        public static bool HasGyroscope
        {
            get
            {
                return SystemInfo.supportsGyroscope;
            }
        }

        public static Quaternion Get()
        {
            if (!gyro_Initialized)
                InitGyro();

            return HasGyroscope ? ReadGryoscopeRotation() : Quaternion.identity;
        }

        private static void InitGyro()
        {
            if (HasGyroscope == true)
            {
                Input.gyro.enabled = true;
                Input.gyro.updateInterval = 0.0167f; // 60 Frame
                //Input.gyro.updateInterval   = 0.0083f; // 120 Frame
                //Input.gyro.updateInterval   = 0.0333f; // 30 Frame
            }

            eulerAngle = Vector3.zero;
            gyro_Initialized = true;
        }

        private static Quaternion ReadGryoscopeRotation()
        {
            Quaternion transquat = Quaternion.identity;
            transquat.w = Input.gyro.attitude.w;
            transquat.x = -Input.gyro.attitude.x;
            transquat.y = -Input.gyro.attitude.y;
            transquat.z = Input.gyro.attitude.z;

            return Quaternion.Euler(90.0f, 0.0f, 0.0f) * transquat;

            //return new Quaternion(-0.5f, -0.5f, 0.5f, -0.5f) * Input.gyro.attitude * new Quaternion(0, 0, 1, 0);
        }
    }
}
