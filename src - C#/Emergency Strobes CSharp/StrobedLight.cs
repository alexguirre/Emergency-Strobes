namespace Emergency_Strobes
{
    // System
    using System;

    // RPH
    using Rage;

    internal class StrobedLight
    {
        public const float DeformationThreshold = 0.0022225f;


        public readonly StrobedVehicle StrobedVehicle;
        public readonly VehicleLight Light;
        
        private bool shouldLightBeBroken;
        public Vector3 lightOffset;

        public StrobedLight(StrobedVehicle vehicle, VehicleLight light)
        {
            StrobedVehicle = vehicle;
            Light = light;
            lightOffset = GetLightOffsetFor(vehicle.Vehicle, light);
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                if (!shouldLightBeBroken)
                    StrobedVehicle.Vehicle.SetLightBroken(Light, false);
            }
            else
            {
                StrobedVehicle.Vehicle.SetLightBroken(Light, true);
            }
        }

        public void CheckLightDeformation()
        {
            shouldLightBeBroken = StrobedVehicle.Vehicle.GetDeformationAt(lightOffset).LengthSquared() > DeformationThreshold;
        }


        private static Vector3 GetLightOffsetFor(Vehicle vehicle, VehicleLight light)
        {
            string lightBoneName = GetBoneNameForLight(light);
            if (lightBoneName == null)
                throw new ArgumentOutOfRangeException(nameof(light));

            Vector3 offset = Vector3.Zero;
            Vector3 boneWorldPos = vehicle.HasBone(lightBoneName) ? vehicle.GetBonePosition(lightBoneName) : Vector3.Zero;

            // if a light is broken it returns a position Vector3 near Vector3.Zero, if so get an approximate offset
            if (Vector3.DistanceSquared(boneWorldPos, Vector3.Zero) < 1.25f * 1.25f)
            {
                string lightName = light.ToString().ToLower();

                // left or right
                Vector3 posOffset1 = vehicle.GetPositionOffset(lightName.Contains("left") ? vehicle.LeftPosition : vehicle.RightPosition);
                // front or rear
                Vector3 posOffset2 = vehicle.GetPositionOffset(lightName.Contains("headlight") ? vehicle.FrontPosition : vehicle.RearPosition);

                offset = new Vector3(posOffset1.X, posOffset2.Y, posOffset2.Z);
            }
            else
            {
                offset = vehicle.GetPositionOffset(boneWorldPos);
            }

            return offset;
        }

        private static string GetBoneNameForLight(VehicleLight light)
        {
            switch (light)
            {
                case VehicleLight.LeftHeadlight: return "headlight_l";
                case VehicleLight.RightHeadlight: return "headlight_r";
                case VehicleLight.LeftTailLight: return "taillight_l";
                case VehicleLight.RightTailLight: return "taillight_r";
                case VehicleLight.LeftBrakeLight: return "brakelight_l";
                case VehicleLight.RightBrakeLight: return "brakelight_r";
                case VehicleLight.MiddleBrakeLight: return "brakelight_m";
                default: return null;
            }
        }
    }
}
