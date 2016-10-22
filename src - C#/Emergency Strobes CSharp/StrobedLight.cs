namespace Emergency_Strobes
{
    // System
    using System;

    // RPH
    using Rage;
    using Rage.Native;

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

            //Vector3 headlightLeftPos = Vehicle.HasBone("headlight_l") ? Vehicle.GetBonePosition("headlight_l") : Vector3.Zero;
            //Vector3 headlightRightPos = Vehicle.HasBone("headlight_r") ? Vehicle.GetBonePosition("headlight_r") : Vector3.Zero;

            //// if a headlight is broken it returns position Vector3 around Vector3.Zero, if so get an approximate offset
            //if (headlightLeftPos.DistanceTo(Vector3.Zero) < 1.25f)
            //{
            //    Vector3 leftPosOffset = Vehicle.GetPositionOffset(Vehicle.LeftPosition);
            //    Vector3 frontPosOffset = Vehicle.GetPositionOffset(Vehicle.FrontPosition);

            //    leftHeadlightOffset = new Vector3(leftPosOffset.X, frontPosOffset.Y, frontPosOffset.Z);
            //}
            //else
            //{
            //    leftHeadlightOffset = Vehicle.GetPositionOffset(headlightLeftPos);
            //}

            //if (headlightRightPos.DistanceTo(Vector3.Zero) < 1.25f)
            //{
            //    Vector3 rightPosOffset = Vehicle.GetPositionOffset(Vehicle.RightPosition);
            //    Vector3 frontPosOffset = Vehicle.GetPositionOffset(Vehicle.FrontPosition);

            //    rightHeadlightOffset = new Vector3(rightPosOffset.X, frontPosOffset.Y, frontPosOffset.Z);
            //}
            //else
            //{
            //    rightHeadlightOffset = Vehicle.GetPositionOffset(headlightRightPos);
            //}



            //Vector3 tailLightLeftPos = Vehicle.HasBone("taillight_l") ? Vehicle.GetBonePosition("taillight_l") : Vector3.Zero;
            //Vector3 tailLightRightPos = Vehicle.HasBone("taillight_r") ? Vehicle.GetBonePosition("taillight_r") : Vector3.Zero;

            //// if a headlight is broken it returns position Vector3 around Vector3.Zero, if so get an approximate offset
            //if (tailLightLeftPos.DistanceTo(Vector3.Zero) < 1.25f)
            //{
            //    Vector3 leftPosOffset = Vehicle.GetPositionOffset(Vehicle.LeftPosition);
            //    Vector3 rearPosOffset = Vehicle.GetPositionOffset(Vehicle.RearPosition);

            //    leftTailLightOffset = new Vector3(leftPosOffset.X, rearPosOffset.Y, rearPosOffset.Z);
            //}
            //else
            //{
            //    leftTailLightOffset = Vehicle.GetPositionOffset(tailLightLeftPos);
            //}

            //if (headlightRightPos.DistanceTo(Vector3.Zero) < 1.25f)
            //{
            //    Vector3 rightPosOffset = Vehicle.GetPositionOffset(Vehicle.RightPosition);
            //    Vector3 rearPosOffset = Vehicle.GetPositionOffset(Vehicle.RearPosition);

            //    rightTailLightOffset = new Vector3(rightPosOffset.X, rearPosOffset.Y, rearPosOffset.Z);
            //}
            //else
            //{
            //    rightTailLightOffset = Vehicle.GetPositionOffset(tailLightRightPos);
            //}



            //Vector3 brakeLightLeftPos = Vehicle.HasBone("brakelight_l") ? Vehicle.GetBonePosition("brakelight_l") : Vector3.Zero;
            //Vector3 brakeLightRightPos = Vehicle.HasBone("brakelight_r") ? Vehicle.GetBonePosition("brakelight_r") : Vector3.Zero;

            //// if a headlight is broken it returns position Vector3 around Vector3.Zero, if so get an approximate offset
            //if (brakeLightLeftPos.DistanceTo(Vector3.Zero) < 1.25f)
            //{
            //    Vector3 leftPosOffset = Vehicle.GetPositionOffset(Vehicle.LeftPosition);
            //    Vector3 rearPosOffset = Vehicle.GetPositionOffset(Vehicle.RearPosition);

            //    leftBrakeLightOffset = new Vector3(leftPosOffset.X, rearPosOffset.Y, rearPosOffset.Z);
            //}
            //else
            //{
            //    leftBrakeLightOffset = Vehicle.GetPositionOffset(brakeLightLeftPos);
            //}

            //if (brakeLightRightPos.DistanceTo(Vector3.Zero) < 1.25f)
            //{
            //    Vector3 rightPosOffset = Vehicle.GetPositionOffset(Vehicle.RightPosition);
            //    Vector3 rearPosOffset = Vehicle.GetPositionOffset(Vehicle.RearPosition);

            //    rightBrakeLightOffset = new Vector3(rightPosOffset.X, rearPosOffset.Y, rearPosOffset.Z);
            //}
            //else
            //{
            //    rightBrakeLightOffset = Vehicle.GetPositionOffset(brakeLightRightPos);
            //}
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
                default: return null;
            }
        }

        //private static float GetDeformationThresholdForLight(VehicleLight light)
        //{
        //    switch (light)
        //    {
        //        case VehicleLight.LeftHeadlight:
        //        case VehicleLight.RightHeadlight:
        //            return HeadlightsDeformationThreshold;

        //        case VehicleLight.LeftTailLight:
        //        case VehicleLight.RightTailLight:
        //            return TailLightsDeformationThreshold;

        //        case VehicleLight.LeftBrakeLight:
        //        case VehicleLight.RightBrakeLight:
        //            return BrakeLightsDeformationThreshold;
        //    }
        //    return 0.0f;
        //}
    }
}

//        LeftHeadlight = 1 << 0,
//        RightHeadlight = 1 << 1,

//        LeftTailLight = 1 << 2,
//        RightTailLight = 1 << 3,

//        LeftBrakeLight = 1 << 8,
//        RightBrakeLight = 1 << 9,

//        All = LeftHeadlight | RightHeadlight | LeftTailLight | RightTailLight | LeftBrakeLight | RightBrakeLight,
