namespace Emergency_Strobes
{
    // System
    using System.Windows.Forms;

    // RPH
    using Rage;
    using Rage.Native;

    internal class PlayerStrobedVehicle
    {
        public readonly Vehicle Vehicle;
        public Pattern Pattern;
        public int PatternIndex;

        private int stagesCount;

        private Pattern.Stage currentStage;
        private int currentStageIndex;
        private int currentStageRemainingTicks;

        private bool active;
        private bool manuallyActive;
        private bool manualDisable;

        public PlayerStrobedVehicle(Vehicle veh, int patternIndex)
        {
            Vehicle = veh;
            PatternIndex = patternIndex;
            Pattern = Settings.Patterns[PatternIndex];
            stagesCount = Pattern.Stages.Length;
        }

        public void ChangePattern(int newPatternIndex)
        {
            PatternIndex = newPatternIndex;
            Pattern = Settings.Patterns[PatternIndex];
            Game.DisplaySubtitle($"~b~[Emergency Strobes]~s~ Switching to pattern ~y~{Pattern.Name}~s~");
            stagesCount = Pattern.Stages.Length;
            ChangeStage(0);
            if (active || manuallyActive)
                UpdateVehicleToCurrentStage();
        }

        public void Update()
        {
            bool prevActive = active;
            active = Vehicle.IsSirenOn;

            bool prevManuallyActive = manuallyActive;
            if (!active && Game.IsKeyDown(Settings.ToggleKey))
                manuallyActive = !manuallyActive;


            if (active != prevActive || manuallyActive != prevManuallyActive)
            {
                if (!active)
                    manualDisable = false;

                if (active || manuallyActive)
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

            bool prevManualDisable = manualDisable;
            if (active && Game.IsKeyDown(Settings.ToggleKey))
                manualDisable = !manualDisable;

            if (manualDisable != prevManualDisable)
            {
                if (manualDisable)
                {
                    Vehicle.SetLeftHeadlightBroken(false);
                    Vehicle.SetRightHeadlightBroken(false);
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 0);
                }
                else
                {
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 2);
                    UpdateVehicleToCurrentStage();
                }
            }

            if ((active || manuallyActive) && !manualDisable)
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

            if (Game.IsKeyDown(Settings.SwitchPatternKey))
            {
                int newIndex = PatternIndex + 1;
                if (newIndex >= Settings.Patterns.Length)
                    newIndex = 0;

                ChangePattern(newIndex);
            }
        }

        public void Reset()
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
