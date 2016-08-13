namespace Emergency_Strobes
{
    // RPH
    using Rage;
    using Rage.Native;

    internal static class Extensions
    {
        public static unsafe void SetLeftHeadlightBroken(this Vehicle v, bool broken)
        {
            byte mask = 1 << 0;

            if (broken)
            {
                *(int*)(v.MemoryAddress.ToInt64() + 1916) |= mask;
            }
            else
            {
                *(int*)(v.MemoryAddress.ToInt64() + 1916) &= ~mask;
            }
        }

        public static unsafe void SetRightHeadlightBroken(this Vehicle v, bool broken)
        {
            byte mask = 1 << 1;

            if (broken)
            {
                *(int*)(v.MemoryAddress.ToInt64() + 1916) |= mask;
            }
            else
            {
                *(int*)(v.MemoryAddress.ToInt64() + 1916) &= ~mask;
            }
        }

        public static bool IsLeftHeadlightBroken(this Vehicle v)
        {
            return NativeFunction.Natives.GetIsLeftVehicleHeadlightDamaged<bool>(v);
        }

        public static bool IsRightHeadlightBroken(this Vehicle v)
        {
            return NativeFunction.Natives.GetIsRightVehicleHeadlightDamaged<bool>(v);
        }
    }
}
