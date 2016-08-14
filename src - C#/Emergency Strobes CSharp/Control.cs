namespace Emergency_Strobes
{
    // RPH
    using Rage;

    internal enum Control
    {
        Toggle,
        SwitchPattern,
    }

    internal static class ControlExtensions
    {
        public static bool IsJustPressed(this Control c)
        {
            bool modifier = Settings.ModifierKey == System.Windows.Forms.Keys.None ? true : Game.IsKeyDownRightNow(Settings.ModifierKey);

            if (modifier)
            {
                bool key = false;

                switch (c)
                {
                    case Control.Toggle:
                        key = Game.IsKeyDown(Settings.ToggleKey);
                        break;
                    case Control.SwitchPattern:
                        key = Game.IsKeyDown(Settings.SwitchPatternKey);
                        break;
                }

                return modifier && key;
            }

            return false;
        }
    }
}
