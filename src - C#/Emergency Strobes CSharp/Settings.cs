namespace Emergency_Strobes
{
    // System
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using System.Windows.Forms;
    using System.Media;

    // RPH
    using Rage;

    internal static class Settings
    {
        public static readonly InitializationFile INIFile = new InitializationFile(@"Plugins\Emergency Strobes Configuration.ini", false);

        public static readonly Model[] ExcludedVehicleModels = Array.ConvertAll(INIFile.ReadString("General", "Excluded Vehicle Models", "").Split(','), (s) => { return new Model(s); } );

        public static readonly Pattern[] Patterns;

        public static readonly bool AIEnabled = INIFile.ReadBoolean("General", "AI", true);
        public static readonly bool PlayerEnabled = INIFile.ReadBoolean("General", "Player", true);

        public static readonly Keys ModifierKey = INIFile.ReadEnum("Keys", "Modifier", Keys.None);
        public static readonly Keys ToggleKey = INIFile.ReadEnum("Keys", "Toggle", Keys.T);
        public static readonly Keys SwitchPatternKey = INIFile.ReadEnum("Keys", "Switch Pattern", Keys.F8);

        public static readonly ControllerButtons ModifierButton = INIFile.ReadEnum("ControllerButtons", "Modifier", ControllerButtons.None);
        public static readonly ControllerButtons ToggleButton = INIFile.ReadEnum("ControllerButtons", "Toggle", ControllerButtons.None);
        public static readonly ControllerButtons SwitchPatternButton = INIFile.ReadEnum("ControllerButtons", "Switch Pattern", ControllerButtons.None);

        public static readonly bool ShowUI = INIFile.ReadBoolean("UI", "Show", true);
        public static readonly string UIFontName = INIFile.ReadString("UI", "Font", "Stencil Std");

        public static readonly bool PlaySwitchSounds = INIFile.ReadBoolean("Sounds", "Switch Pattern", true);

        public static readonly SoundPlayer SwitchSound;

        static Settings()
        {
            if (File.Exists(@"Plugins\Emergency Strobes Patterns.xml"))
            {
                XmlSerializer s = new XmlSerializer(typeof(Pattern[]));
                using (StreamReader reader = new StreamReader(@"Plugins\Emergency Strobes Patterns.xml"))
                {
                    Patterns = (Pattern[])s.Deserialize(reader);
                }
            }
            else
            {
                Patterns = new Pattern[]
                {
                    new Pattern("Left-Right Slow", new Pattern.Stage[]
                    {
                        new Pattern.Stage(PatternStageType.LeftHeadlight, 210),
                        new Pattern.Stage(PatternStageType.BothHeadlights, 210),
                        new Pattern.Stage(PatternStageType.RightHeadlight, 210),
                        new Pattern.Stage(PatternStageType.BothHeadlights, 210),
                    }),

                    new Pattern("Left-Right Slow 2", new Pattern.Stage[]
                    {
                        new Pattern.Stage(PatternStageType.LeftHeadlight, 210),
                        new Pattern.Stage(PatternStageType.None, 180),
                        new Pattern.Stage(PatternStageType.LeftHeadlight, 210),
                        new Pattern.Stage(PatternStageType.BothHeadlights, 260),
                        new Pattern.Stage(PatternStageType.RightHeadlight, 210),
                        new Pattern.Stage(PatternStageType.None, 180),
                        new Pattern.Stage(PatternStageType.RightHeadlight, 210),
                        new Pattern.Stage(PatternStageType.BothHeadlights, 260),
                    }),

                    new Pattern("Both Slow", new Pattern.Stage[]
                    {
                        new Pattern.Stage(PatternStageType.None, 210),
                        new Pattern.Stage(PatternStageType.BothHeadlights, 230),
                        new Pattern.Stage(PatternStageType.None, 230),
                        new Pattern.Stage(PatternStageType.BothHeadlights, 210),
                    }),

                    new Pattern("Both Fast", new Pattern.Stage[]
                    {
                        new Pattern.Stage(PatternStageType.None, 160),
                        new Pattern.Stage(PatternStageType.BothHeadlights, 110),
                        new Pattern.Stage(PatternStageType.None, 160),
                        new Pattern.Stage(PatternStageType.BothHeadlights, 110),
                    }),

                    new Pattern("Left-Right Fast", new Pattern.Stage[]
                    {
                        new Pattern.Stage(PatternStageType.LeftHeadlight, 110),
                        new Pattern.Stage(PatternStageType.BothHeadlights, 110),
                        new Pattern.Stage(PatternStageType.RightHeadlight, 110),
                        new Pattern.Stage(PatternStageType.BothHeadlights, 110),
                    }),

                    new Pattern("Left-Right Fast 2", new Pattern.Stage[]
                    {
                        new Pattern.Stage(PatternStageType.LeftHeadlight, 150),
                        new Pattern.Stage(PatternStageType.None, 115),
                        new Pattern.Stage(PatternStageType.LeftHeadlight, 150),
                        new Pattern.Stage(PatternStageType.BothHeadlights, 130),
                        new Pattern.Stage(PatternStageType.RightHeadlight, 150),
                        new Pattern.Stage(PatternStageType.None, 115),
                        new Pattern.Stage(PatternStageType.RightHeadlight, 150),
                        new Pattern.Stage(PatternStageType.BothHeadlights, 130),
                    }),
                };

                XmlSerializer s = new XmlSerializer(typeof(Pattern[]));
                using (StreamWriter writer = new StreamWriter(@"Plugins\Emergency Strobes Patterns.xml", false))
                {
                    s.Serialize(writer, Patterns);
                }
            }

            if (PlaySwitchSounds)
            {
                if (File.Exists(@"Plugins\Emergency Strobes\switch.wav"))
                    SwitchSound = new SoundPlayer(@"Plugins\Emergency Strobes\switch.wav");
                else
                    SwitchSound = new SoundPlayer(Emergency_Strobes.Properties.Resources._switch);
            }
        }
    }
}
