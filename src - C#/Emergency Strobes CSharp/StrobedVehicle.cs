namespace Emergency_Strobes
{
    // System
    using System;

    // RPH
    using Rage;
    using Rage.Native;

    internal class StrobedVehicle : IDisposable
    {
        protected struct StageData
        {
            public Pattern.Stage Stage;
            public int Index;
            public uint StartTime;
            public bool ShouldEnableBrakeLights;
        }

        public readonly Vehicle Vehicle;
        protected Pattern pattern;
        public virtual Pattern Pattern
        {
            get { return pattern; }
            set
            {
                pattern = value;
                stagesCount = Pattern.Stages.Length;
                ChangeStage(0);
                if (ShouldDoActiveUpdate())
                    UpdateVehicleToCurrentStage();
            }
        }

        public StrobedLight LeftHeadlight { get; }
        public StrobedLight RightHeadlight { get; }

        public StrobedLight LeftTailLight { get; }
        public StrobedLight RightTailLight { get; }

        public StrobedLight LeftBrakeLight { get; }
        public StrobedLight RightBrakeLight { get; }

        protected int stagesCount;

        protected StageData currentStageData;

        protected bool prevActive;
        protected bool active;

        protected bool disabledHeadTailLightsSequences;

        public StrobedVehicle(Vehicle veh, Pattern pattern)
        {
            Vehicle = veh;
            Pattern = pattern;
            stagesCount = Pattern.Stages.Length;

            LeftHeadlight = new StrobedLight(this, VehicleLight.LeftHeadlight);
            RightHeadlight = new StrobedLight(this, VehicleLight.RightHeadlight);

            LeftTailLight = new StrobedLight(this, VehicleLight.LeftTailLight);
            RightTailLight = new StrobedLight(this, VehicleLight.RightTailLight);

            LeftBrakeLight = new StrobedLight(this, VehicleLight.LeftBrakeLight);
            RightBrakeLight = new StrobedLight(this, VehicleLight.RightBrakeLight);

            // with custom siren settings RPH fails to obtain the Vehicle's EmergencyLighting, if so don't disable the headlight and tail lights sequences
            EmergencyLighting defaultEmergencyLighting = Vehicle.DefaultEmergencyLighting;

            if (defaultEmergencyLighting == null)
            {
                defaultEmergencyLighting = Vehicle.EmergencyLighting;

                if (defaultEmergencyLighting == null)
                {
                    defaultEmergencyLighting = Vehicle.Model.EmergencyLighting;
                }
            }

            if (defaultEmergencyLighting != null)
            {
                Vehicle.EmergencyLightingOverride = defaultEmergencyLighting.Clone();
                Vehicle.EmergencyLightingOverride.RightHeadLightSequenceRaw = 0;
                Vehicle.EmergencyLightingOverride.RightTailLightSequenceRaw = 0;
                Vehicle.EmergencyLightingOverride.LeftHeadLightSequenceRaw = 0;
                Vehicle.EmergencyLightingOverride.LeftTailLightSequenceRaw = 0;
                disabledHeadTailLightsSequences = true;
            }
            else
            {
                disabledHeadTailLightsSequences = false;
            }

            Game.LogTrivial((disabledHeadTailLightsSequences ?
                            "Successfully disabled game's headlights and tail lights sequences for model " + veh.Model.Name :
                            "Failed to disabled game's headlights and tail lights sequences for model " + veh.Model.Name));
        }

        public virtual void Update()
        {
            prevActive = active;
            active = Vehicle.IsSirenOn;

            if (active != prevActive)
            {
                SetActive(active);
            }

            if (ShouldDoActiveUpdate())
            {
                ActiveUpdate();
            }
        }

        public void ResetVehicleLights()
        {
            if (Vehicle)
            {
                LeftHeadlight.SetActive(true);
                RightHeadlight.SetActive(true);

                LeftTailLight.SetActive(true);
                RightTailLight.SetActive(true);

                LeftBrakeLight.SetActive(true);
                RightBrakeLight.SetActive(true);

                NativeFunction.Natives.SetVehicleLights(Vehicle, 0);
                Vehicle.SetLightMultiplier(1.0f);
            }
        }

        protected virtual void SetActive(bool activate)
        {
            if (activate)
            {
                NativeFunction.Natives.SetVehicleLights(Vehicle, 2);
                Vehicle.SetLightMultiplier(Settings.BrightnessMultiplier);
                CheckLightsDeformation();
                UpdateVehicleToCurrentStage();
            }
            else
            {
                ResetVehicleLights();
            }
        }

        protected virtual void ActiveUpdate()
        {
            if (NeedsToChangeStage())
            {
                CheckLightsDeformation();

                int newStageIndex = currentStageData.Index + 1;
                if (newStageIndex >= stagesCount)
                    newStageIndex = 0;

                ChangeStage(newStageIndex);

                UpdateVehicleToCurrentStage();
            }

            if (Vehicle.AreBrokenLightsRenderedAsBroken())
                Vehicle.SetBrokenLightsRenderedAsBroken(false);

            if (currentStageData.ShouldEnableBrakeLights)
            {
                NativeFunction.Natives.SetVehicleBrakeLights(Vehicle, true);
            }


            //Vehicle.IsEngineOn = true; // sometimes the lights turn off when exiting the vehicle, this fixes it
        }

        protected virtual bool ShouldDoActiveUpdate()
        {
            return active;
        }

        protected void ChangeStage(int newIndex)
        {
            Pattern.Stage currentStage = Pattern.Stages[newIndex];
            currentStageData = new StageData()
            {
                Stage = currentStage,
                Index = newIndex,
                StartTime = Plugin.GameTime,
                ShouldEnableBrakeLights = ShouldEnableVehicleBrakeLightsForStage(currentStage)
            };
        }

        protected void UpdateVehicleToCurrentStage()
        {
            PatternStageType type = currentStageData.Stage.Type;

            if (type == PatternStageType.None)
            {
                LeftHeadlight.SetActive(false);
                RightHeadlight.SetActive(false);

                LeftTailLight.SetActive(false);
                RightTailLight.SetActive(false);

                LeftBrakeLight.SetActive(false);
                RightBrakeLight.SetActive(false);
                return;
            }


            LeftHeadlight.SetActive((type & PatternStageType.LeftHeadlight) == PatternStageType.LeftHeadlight);
            RightHeadlight.SetActive((type & PatternStageType.RightHeadlight) == PatternStageType.RightHeadlight);

            LeftTailLight.SetActive((type & PatternStageType.LeftTailLight) == PatternStageType.LeftTailLight);
            RightTailLight.SetActive((type & PatternStageType.RightTailLight) == PatternStageType.RightTailLight);

            LeftBrakeLight.SetActive((type & PatternStageType.LeftBrakeLight) == PatternStageType.LeftBrakeLight);
            RightBrakeLight.SetActive((type & PatternStageType.RightBrakeLight) == PatternStageType.RightBrakeLight);
        }

        protected bool NeedsToChangeStage()
        {
            return Plugin.GameTime - currentStageData.StartTime > currentStageData.Stage.Milliseconds;
        }

        protected void CheckLightsDeformation()
        {
            LeftHeadlight.CheckLightDeformation();
            RightHeadlight.CheckLightDeformation();

            LeftTailLight.CheckLightDeformation();
            RightTailLight.CheckLightDeformation();

            LeftBrakeLight.CheckLightDeformation();
            RightBrakeLight.CheckLightDeformation();
        }

        public virtual void Dispose()
        {
            if (Vehicle)
            {
                ResetVehicleLights();
                // with custom siren settings, setting EmergencyLightingOverride to null removes all EmergencyLighting from the Vehicle(the siren no longer works)
                if (disabledHeadTailLightsSequences) 
                {
                    Vehicle.EmergencyLightingOverride = null;
                }
            }
        }


        private static bool ShouldEnableVehicleBrakeLightsForStage(Pattern.Stage stage)
        {
            PatternStageType t = stage.Type;
            if ((t & PatternStageType.LeftTailLight) == PatternStageType.LeftTailLight ||
               (t & PatternStageType.RightTailLight) == PatternStageType.RightTailLight ||
               (t & PatternStageType.LeftBrakeLight) == PatternStageType.LeftBrakeLight ||
               (t & PatternStageType.RightBrakeLight) == PatternStageType.RightBrakeLight)
                return true;
            return false;
        }
    }
}
