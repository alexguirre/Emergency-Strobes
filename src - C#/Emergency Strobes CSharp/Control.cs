namespace Emergency_Strobes
{
    // System
    using System.Windows.Forms;

    // RPH
    using Rage;
    using Rage.Native;

    internal enum Control
    {
        Toggle,
        SwitchPattern,
    }

    internal static class ControlExtensions
    {
        private static bool IsUsingController => !NativeFunction.CallByHash<bool>(0xa571d46727e2b718, 2);

        public static bool IsJustPressed(this Control c)
        {
            if (IsUsingController)
            {
                bool modifier = (Settings.ModifierButton == ControllerButtons.None ? true : Game.IsControllerButtonDownRightNow(Settings.ModifierButton));

                if (modifier)
                {
                    bool key = false;

                    switch (c)
                    {
                        case Control.Toggle:
                            key = (Settings.ToggleButton == ControllerButtons.None ? false : Game.IsControllerButtonDown(Settings.ToggleButton));
                            break;
                        case Control.SwitchPattern:
                            key = (Settings.SwitchPatternButton == ControllerButtons.None ? false : Game.IsControllerButtonDown(Settings.SwitchPatternButton));
                            break;
                    }

                    return modifier && key;
                }
            }
            else
            {
                bool modifier = (Settings.ModifierKey == Keys.None ? true : Game.IsKeyDownRightNow(Settings.ModifierKey));

                if (modifier)
                {
                    bool key = false;

                    switch (c)
                    {
                        case Control.Toggle:
                            key = (Settings.ToggleKey == Keys.None ? false : Game.IsKeyDown(Settings.ToggleKey));
                            break;
                        case Control.SwitchPattern:
                            key = (Settings.SwitchPatternKey == Keys.None ? false : Game.IsKeyDown(Settings.SwitchPatternKey));
                            break;
                    }

                    return modifier && key;
                }
            }

            return false;
        }
    }
}
