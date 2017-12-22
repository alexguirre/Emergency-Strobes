namespace Emergency_Strobes
{
    using System;

    using Rage;
    using Rage.Native;

    [Flags]
    internal enum VehicleLight
    {
        LeftHeadlight = 1 << 0,
        RightHeadlight = 1 << 1,

        LeftTailLight = 1 << 2,
        RightTailLight = 1 << 3,

        LeftFrontIndicatorLight = 1 << 4,
        RightFrontIndicatorLight = 1 << 5,
        LeftRearIndicatorLight = 1 << 6,
        RightRearIndicatorLight = 1 << 7,

        LeftBrakeLight = 1 << 8,
        RightBrakeLight = 1 << 9,
        MiddleBrakeLight = 1 << 10,

        All = LeftHeadlight | RightHeadlight | LeftTailLight | RightTailLight | LeftBrakeLight | RightBrakeLight | LeftFrontIndicatorLight | RightFrontIndicatorLight | LeftRearIndicatorLight | RightRearIndicatorLight | MiddleBrakeLight,
    }

    internal static class Extensions
    {
        private static readonly int BrokenLightsOffset;
        private static readonly int LightMultiplierOffset;
        private static readonly int ShouldRenderBrokenLightsOffset;

        static Extensions()
        {
            switch (Game.ProductVersion.Build)
            {
                default:
                case 1290:
                    BrokenLightsOffset = 0x080C;
                    LightMultiplierOffset = 0x09A4;
                    ShouldRenderBrokenLightsOffset = 0x0814;
                    break;
                case 1180:
                    BrokenLightsOffset = 0x07EC;
                    LightMultiplierOffset = 0x0984;
                    ShouldRenderBrokenLightsOffset = 0x07F4;
                    break;
                case 1103:
                    BrokenLightsOffset = 0x07CC;
                    LightMultiplierOffset = 0x0964;
                    ShouldRenderBrokenLightsOffset = 0x07D4;
                    break;
                case 1032:
                case 1011:
                case 944:
                    BrokenLightsOffset = 0x07BC;
                    LightMultiplierOffset = 0x0954;
                    ShouldRenderBrokenLightsOffset = 0x07C4;
                    break;
                case 877:
                    BrokenLightsOffset = 0x079C;
                    LightMultiplierOffset = 0x092C;
                    ShouldRenderBrokenLightsOffset = 0x07A4;
                    break;
                case 791:
                    BrokenLightsOffset = 0x077C;
                    LightMultiplierOffset = 0x090C;
                    ShouldRenderBrokenLightsOffset = 0x0784;
                    break;
            }
        }

        public static unsafe void SetLightBroken(this Vehicle v, VehicleLight light, bool broken)
        {
            int mask = (int)light;

            if (broken)
            {
                *(int*)(v.MemoryAddress.ToInt64() + BrokenLightsOffset) |= mask;
            }
            else
            {
                *(int*)(v.MemoryAddress.ToInt64() + BrokenLightsOffset) &= ~mask;
            }
        }

        public static unsafe bool IsLightBroken(this Vehicle v, VehicleLight light)
        {
            return (*(int*)(v.MemoryAddress.ToInt64() + BrokenLightsOffset) & (int)light) == (int)light;
        }

        public static Vector3 GetDeformationAt(this Vehicle v, Vector3 offset)
        {
            return NativeFunction.Natives.GetVehicleDeformationAtPos<Vector3>(v, offset.X, offset.Y, offset.Z);
        }

        public static unsafe void SetLightMultiplier(this Vehicle v, float multiplier)
        {
            //NativeFunction.Natives.SetVehicleLightMultiplier(v, multiplier);
            *(float*)(v.MemoryAddress + LightMultiplierOffset) = multiplier;
        }

        public static unsafe float GetLightMultiplier(this Vehicle v)
        {
            return *(float*)(v.MemoryAddress + LightMultiplierOffset);
        }

        public static unsafe void SetBrokenLightsRenderedAsBroken(this Vehicle v, bool broken)
        {
            *(byte*)(v.MemoryAddress + ShouldRenderBrokenLightsOffset) = (byte)(broken ? 1 : 0);
        }

        public static unsafe bool AreBrokenLightsRenderedAsBroken(this Vehicle v)
        {
            byte value = *(byte*)(v.MemoryAddress + ShouldRenderBrokenLightsOffset);
            return value == 1;
        }
    }
}
