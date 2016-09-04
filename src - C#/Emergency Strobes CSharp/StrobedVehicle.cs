namespace Emergency_Strobes
{
    // RPH
    using Rage;
    using Rage.Native;

    internal class StrobedVehicle
    {
        public const float HeadlightsDeformationThreshold = 0.0022225f;

        public readonly Vehicle Vehicle;
        public Pattern Pattern;

        private int stagesCount;

        private Pattern.Stage currentStage;
        private int currentStageIndex;
        private uint currentStageStartTime;

        private bool active;

        private bool shouldLeftHeadlightBeBroken, shouldRightHeadlightBeBroken;
        private Vector3 leftHeadlightOffset, rightHeadlightOffset;

        public StrobedVehicle(Vehicle veh, Pattern pattern)
        {
            Vehicle = veh;
            Pattern = pattern;
            stagesCount = Pattern.Stages.Length;

            Vector3 headlightLeftPos = veh.HasBone("headlight_l") ? veh.GetBonePosition("headlight_l") : Vector3.Zero;
            Vector3 headlightRightPos = veh.HasBone("headlight_r") ? veh.GetBonePosition("headlight_r") : Vector3.Zero;

            // if a headlight is broken it returns position Vector3 around Vector3.Zero, if so get an approximate offset
            if (headlightLeftPos.DistanceTo(Vector3.Zero) < 1.25f)
            {
                Vector3 leftPosOffset = veh.GetPositionOffset(veh.LeftPosition);
                Vector3 frontPosOffset = veh.GetPositionOffset(veh.FrontPosition);

                leftHeadlightOffset = new Vector3(leftPosOffset.X, frontPosOffset.Y, frontPosOffset.Z);
            }
            else
            {
                leftHeadlightOffset = veh.GetPositionOffset(headlightLeftPos);
            }

            if (headlightRightPos.DistanceTo(Vector3.Zero) < 1.25f)
            {
                Vector3 rightPosOffset = veh.GetPositionOffset(veh.RightPosition);
                Vector3 frontPosOffset = veh.GetPositionOffset(veh.FrontPosition);

                rightHeadlightOffset = new Vector3(rightPosOffset.X, frontPosOffset.Y, frontPosOffset.Z);
            }
            else
            {
                rightHeadlightOffset = veh.GetPositionOffset(headlightRightPos);
            }
        }

        public void ChangePattern(Pattern newPattern)
        {
            Pattern = newPattern;
            stagesCount = Pattern.Stages.Length;
            ChangeStage(0);
            if (active)
                UpdateVehicleToCurrentStage();
        }

        public void Update()
        {
            bool prevActive = active;
            active = Vehicle.IsSirenOn;

            if (active != prevActive)
            {
                if (active)
                {
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 2);
                    shouldLeftHeadlightBeBroken = Vehicle.GetDeformationAt(leftHeadlightOffset).LengthSquared() > HeadlightsDeformationThreshold;
                    shouldRightHeadlightBeBroken = Vehicle.GetDeformationAt(rightHeadlightOffset).LengthSquared() > HeadlightsDeformationThreshold;
                    UpdateVehicleToCurrentStage();
                }
                else
                {
                    if(!shouldLeftHeadlightBeBroken)
                        Vehicle.SetLeftHeadlightBroken(false);
                    if (!shouldRightHeadlightBeBroken)
                        Vehicle.SetRightHeadlightBroken(false);
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 0);
                    //Vehicle.SetLightMultiplier(1.0f);
                }
                
            }

            if (active)
            {
                if (NeedsToChangeStage())
                {
                    shouldLeftHeadlightBeBroken = Vehicle.GetDeformationAt(leftHeadlightOffset).LengthSquared() > HeadlightsDeformationThreshold;
                    shouldRightHeadlightBeBroken = Vehicle.GetDeformationAt(rightHeadlightOffset).LengthSquared() > HeadlightsDeformationThreshold;

                    int newStageIndex = currentStageIndex + 1;
                    if (newStageIndex >= stagesCount)
                        newStageIndex = 0;

                    ChangeStage(newStageIndex);

                    UpdateVehicleToCurrentStage();
                }
            }
        }

        public void ResetVehicleLights()
        {
            if (!shouldLeftHeadlightBeBroken)
                Vehicle.SetLeftHeadlightBroken(false);
            if (!shouldRightHeadlightBeBroken)
                Vehicle.SetRightHeadlightBroken(false);
            NativeFunction.Natives.SetVehicleLights(Vehicle, 0);
            //Vehicle.SetLightMultiplier(1.0f);
        }

        private void ChangeStage(int newIndex)
        {
            currentStageIndex = newIndex;
            currentStage = Pattern.Stages[currentStageIndex];
            currentStageStartTime = EntryPoint.GameTime;
        }

        private void UpdateVehicleToCurrentStage()
        {
            switch (Pattern.Stages[currentStageIndex].Type)
            {
                case PatternStageType.None:
                    Vehicle.SetLeftHeadlightBroken(true);
                    Vehicle.SetRightHeadlightBroken(true);
                    break;
                case PatternStageType.BothHeadlights:
                    if (!shouldLeftHeadlightBeBroken)
                        Vehicle.SetLeftHeadlightBroken(false);
                    if (!shouldRightHeadlightBeBroken)
                        Vehicle.SetRightHeadlightBroken(false);
                    break;
                case PatternStageType.LeftHeadlight:
                    Vehicle.SetLeftHeadlightBroken(true);
                    if (!shouldRightHeadlightBeBroken)
                        Vehicle.SetRightHeadlightBroken(false);
                    break;
                case PatternStageType.RightHeadlight:
                    if (!shouldLeftHeadlightBeBroken)
                        Vehicle.SetLeftHeadlightBroken(false);
                    Vehicle.SetRightHeadlightBroken(true);
                    break;
            }
        }

        private bool NeedsToChangeStage()
        {
            return EntryPoint.GameTime - currentStageStartTime > currentStage.Milliseconds;
        }
    }
}
