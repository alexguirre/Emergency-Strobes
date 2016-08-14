namespace Emergency_Strobes
{
    // System
    using System.Drawing;

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

        public static RectangleF ConvertToCurrentCoordSystem(this RectangleF rectangle)
        {
            Size origRes = Game.Resolution;
            float aspectRatio = origRes.Width / (float)origRes.Height;
            PointF pos = new PointF(rectangle.X / (1080 * aspectRatio), rectangle.Y / 1080f);
            SizeF siz = new SizeF(rectangle.Width / (1080 * aspectRatio), rectangle.Height / 1080f);
            return new RectangleF(pos.X * Game.Resolution.Width, pos.Y * Game.Resolution.Height, siz.Width * Game.Resolution.Width, siz.Height * Game.Resolution.Height);
        }

        public static Vector3 GetDeformationAt(this Vehicle v, Vector3 offset)
        {
            return NativeFunction.Natives.GetVehicleDeformationAtPos<Vector3>(v, offset.X, offset.Y, offset.Z);
        }
    }
}
