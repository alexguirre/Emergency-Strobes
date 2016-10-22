namespace Emergency_Strobes
{
    // System
    using System;
    using System.Linq;

    // RPH
    using Rage;

    internal class PlayerStrobedVehicle : StrobedVehicle
    {
        public Pattern[] Patterns { get; }

        public override Pattern Pattern
        {
            get {  return base.Pattern; }

            set
            {
                base.Pattern = value;
                currentPatternIndex = Patterns == null ? -1 : Array.IndexOf(Patterns, value);
            }
        }

        private int currentPatternIndex;

        private bool prevManuallyActive;
        private bool manuallyActive;

        private bool prevManualDisable;
        private bool manualDisable;

        private bool isPlayerInVehicle;

        public PlayerStrobedVehicle(Vehicle veh, Pattern[] patterns) : base(veh, patterns[0])
        {
            Patterns = patterns;
            currentPatternIndex = 0;
        }

        public override void Update()
        {
            isPlayerInVehicle = Game.LocalPlayer.Character.IsInVehicle(Vehicle, false);
            
            base.Update();

            prevManuallyActive = manuallyActive;
            prevManualDisable = manualDisable;

            if (isPlayerInVehicle)
            {
                if (Control.Toggle.IsJustPressed())
                {
                    if(active)
                        manualDisable = !manualDisable;
                    else
                        manuallyActive = !manuallyActive;
                }
            }

            if (manuallyActive != prevManuallyActive)
                SetActive(manuallyActive);
            else if (manualDisable != prevManualDisable)
                SetActive(!manualDisable);

            if (Control.SwitchPattern.IsJustPressed())
            {
                int newIndex = currentPatternIndex + 1;
                if (newIndex >= Settings.Patterns.Length)
                    newIndex = 0;

                Pattern = Patterns[newIndex];
                Game.DisplaySubtitle($"~b~[Emergency Strobes]~s~ Switching to pattern ~y~{Pattern.Name}~s~");
                if (Settings.PlaySwitchSounds)
                    Settings.SwitchSound.Play();
            }
        }

        protected override void SetActive(bool activate) 
        {
            if (!activate)
                manualDisable = false;

            base.SetActive(activate);
        }

        protected override bool ShouldDoActiveUpdate()
        {
            return (active || manuallyActive) && !manualDisable;
        }
    }
}
