namespace Emergency_Strobes
{
    // RPH
    using Rage;
    using Rage.Native;

    internal class StrobedVehicle
    {
        public readonly Vehicle Vehicle;
        public Pattern Pattern;

        private int stagesCount;

        private Pattern.Stage currentStage;
        private int currentStageIndex;
        private int currentStageRemainingTicks;

        private bool active;

        public StrobedVehicle(Vehicle veh, Pattern pattern)
        {
            Vehicle = veh;
            Pattern = pattern;
            stagesCount = Pattern.Stages.Length;
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
                    //NativeFunction.Natives.SetVehicleLightMultiplier(Vehicle, Settings.Brightness);
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 2);
                    UpdateVehicleToCurrentStage();
                }
                else
                {
                    Vehicle.SetLeftHeadlightBroken(false);
                    Vehicle.SetRightHeadlightBroken(false);
                    //NativeFunction.Natives.SetVehicleLightMultiplier(Vehicle, 1.0f);
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 0);
                }
                
            }

            if (active)
            {
                if (NeedsToChangeStage())
                {
                    int newStageIndex = currentStageIndex + 1;
                    if (newStageIndex >= stagesCount)
                        newStageIndex = 0;

                    ChangeStage(newStageIndex);

                    UpdateVehicleToCurrentStage();
                }

                currentStageRemainingTicks--;
            }
        }

        public void ResetVehicleLights()
        {
            Vehicle.SetLeftHeadlightBroken(false);
            Vehicle.SetRightHeadlightBroken(false);
            //NativeFunction.Natives.SetVehicleLightMultiplier(Vehicle, 1.0f);
            NativeFunction.Natives.SetVehicleLights(Vehicle, 0);
        }

        private void ChangeStage(int newIndex)
        {
            currentStageIndex = newIndex;
            currentStage = Pattern.Stages[currentStageIndex];
            currentStageRemainingTicks = currentStage.Ticks;
        }

        private void UpdateVehicleToCurrentStage()
        {
            switch (Pattern.Stages[currentStageIndex].Type)
            {
                case PatternStageType.None:
                    Vehicle.SetLeftHeadlightBroken(true);
                    Vehicle.SetRightHeadlightBroken(true);
                    break;
                case PatternStageType.Both:
                    Vehicle.SetLeftHeadlightBroken(false);
                    Vehicle.SetRightHeadlightBroken(false);
                    break;
                case PatternStageType.LeftOnly:
                    Vehicle.SetLeftHeadlightBroken(true);
                    Vehicle.SetRightHeadlightBroken(false);
                    break;
                case PatternStageType.RightOnly:
                    Vehicle.SetLeftHeadlightBroken(false);
                    Vehicle.SetRightHeadlightBroken(true);
                    break;
            }
        }

        private bool NeedsToChangeStage()
        {
            return currentStageRemainingTicks <= 0;
        }
    }
}
