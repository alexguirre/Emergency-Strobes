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

        public static readonly bool ShowUI = INIFile.ReadBoolean("UI", "Show", true);
        public static readonly string UIFontName = INIFile.ReadString("UI", "Font", "Stencil Std");

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
                    new Pattern("Pattern1", new Pattern.Stage[]
                    {
                        new Pattern.Stage(PatternStageType.LeftOnly, 30),
                        new Pattern.Stage(PatternStageType.Both, 30),
                        new Pattern.Stage(PatternStageType.RightOnly, 30),
                        new Pattern.Stage(PatternStageType.Both, 30),
                    }),

                    new Pattern("Pattern2", new Pattern.Stage[]
                    {
                        new Pattern.Stage(PatternStageType.LeftOnly, 30),
                        new Pattern.Stage(PatternStageType.None, 20),
                        new Pattern.Stage(PatternStageType.LeftOnly, 30),
                        new Pattern.Stage(PatternStageType.Both, 40),
                        new Pattern.Stage(PatternStageType.RightOnly, 30),
                        new Pattern.Stage(PatternStageType.None, 20),
                        new Pattern.Stage(PatternStageType.RightOnly, 30),
                        new Pattern.Stage(PatternStageType.Both, 40),
                    }),

                    new Pattern("Pattern3", new Pattern.Stage[]
                    {
                        new Pattern.Stage(PatternStageType.None, 25),
                        new Pattern.Stage(PatternStageType.Both, 30),
                        new Pattern.Stage(PatternStageType.None, 25),
                        new Pattern.Stage(PatternStageType.Both, 30),
                    }),

                    new Pattern("Pattern4", new Pattern.Stage[]
                    {
                        new Pattern.Stage(PatternStageType.LeftOnly, 10),
                        new Pattern.Stage(PatternStageType.Both, 10),
                        new Pattern.Stage(PatternStageType.RightOnly, 10),
                        new Pattern.Stage(PatternStageType.Both, 10),
                    }),
                };

                XmlSerializer s = new XmlSerializer(typeof(Pattern[]));
                using (StreamWriter writer = new StreamWriter(@"Plugins\Emergency Strobes Patterns.xml", false))
                {
                    s.Serialize(writer, Patterns);
                }
            }


            if (File.Exists(@"Plugins\Emergency Strobes\switch.wav"))
                SwitchSound = new SoundPlayer(@"Plugins\Emergency Strobes\switch.wav");
            else
                SwitchSound = new SoundPlayer(Emergency_Strobes.Properties.Resources._switch);
        }
    }
}
