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
        public const string PatternsXMLFileName = @"Plugins\Emergency Strobes Patterns.xml";
        public const string SwitchSoundFileName = @"Plugins\Emergency Strobes\switch.wav";

        public static readonly InitializationFile INIFile = new InitializationFile(@"Plugins\Emergency Strobes Configuration.ini", false);

        public static readonly Model[] ExcludedVehicleModels = Array.ConvertAll(INIFile.ReadString("General", "Excluded Vehicle Models", "").Split(','), (s) => { return new Model(s); } );

        public static readonly Pattern[] Patterns;

        public static readonly bool AIEnabled = INIFile.ReadBoolean("General", "AI", true);
        public static readonly bool PlayerEnabled = INIFile.ReadBoolean("General", "Player", true);

        public static readonly float BrightnessMultiplier = INIFile.ReadSingle("General", "Brightness Multiplier", 6);

        public static readonly Keys ModifierKey = INIFile.ReadEnum("Keys", "Modifier", Keys.None);
        public static readonly Keys ToggleKey = INIFile.ReadEnum("Keys", "Toggle", Keys.T);
        public static readonly Keys SwitchPatternKey = INIFile.ReadEnum("Keys", "Switch Pattern", Keys.F8);

        public static readonly ControllerButtons ModifierButton = INIFile.ReadEnum("ControllerButtons", "Modifier", ControllerButtons.None);
        public static readonly ControllerButtons ToggleButton = INIFile.ReadEnum("ControllerButtons", "Toggle", ControllerButtons.None);
        public static readonly ControllerButtons SwitchPatternButton = INIFile.ReadEnum("ControllerButtons", "Switch Pattern", ControllerButtons.None);
        
        public static readonly bool PlaySwitchSounds = INIFile.ReadBoolean("Sounds", "Switch Pattern", true);

        public static readonly SoundPlayer SwitchSound;

        static Settings()
        {
            if (File.Exists(PatternsXMLFileName))
            {
                XmlSerializer s = new XmlSerializer(typeof(Pattern[]));
                using (StreamReader reader = new StreamReader(PatternsXMLFileName))
                {
                    Patterns = (Pattern[])s.Deserialize(reader);
                }
            }
            else
            {
                Patterns = GetDefaultPatterns();

                string xmlText = "";
                using (StringWriter writer = new StringWriter())
                {
                    XmlSerializer s = new XmlSerializer(typeof(Pattern[]));
                    s.Serialize(writer, Patterns);
                    xmlText = writer.ToString().Replace("\"?>", "\"?>" + Environment.NewLine + PatternXMLExplanationText); // add the explanation comment after the XML declaration
                }

                using (StreamWriter writer = new StreamWriter(PatternsXMLFileName, false))
                {
                    writer.Write(xmlText);
                }
            }

            if (PlaySwitchSounds)
            {
                if (File.Exists(SwitchSoundFileName))
                    SwitchSound = new SoundPlayer(SwitchSoundFileName);
                else
                    SwitchSound = new SoundPlayer(Emergency_Strobes.Properties.Resources._switch);
            }
        }


        private static Pattern[] GetDefaultPatterns()
        {
            return new Pattern[]
            {
#if DEBUG
                new Pattern("Test All", new Pattern.Stage[]
                {
                    new Pattern.Stage(PatternStageType.All, 25),
                    new Pattern.Stage(PatternStageType.None, 25),
                }),

                new Pattern("Test", new Pattern.Stage[]
                {
                    new Pattern.Stage(PatternStageType.LeftTailLight | PatternStageType.LeftBrakeLight, 175),
                    new Pattern.Stage(PatternStageType.RightTailLight | PatternStageType.RightBrakeLight, 175),
                    new Pattern.Stage(PatternStageType.None, 200),
                    new Pattern.Stage(PatternStageType.LeftTailLight | PatternStageType.LeftBrakeLight, 175),
                    new Pattern.Stage(PatternStageType.RightTailLight | PatternStageType.RightBrakeLight, 175),
                    new Pattern.Stage(PatternStageType.None, 200),
                    new Pattern.Stage(PatternStageType.LeftTailLight | PatternStageType.LeftBrakeLight, 175),
                    new Pattern.Stage(PatternStageType.RightTailLight | PatternStageType.RightBrakeLight, 175),
                    new Pattern.Stage(PatternStageType.None, 200),
                }),

                new Pattern("Test 2", new Pattern.Stage[]
                {
                    new Pattern.Stage(PatternStageType.LeftTailLight, 175),
                    new Pattern.Stage(PatternStageType.RightBrakeLight, 175),
                    new Pattern.Stage(PatternStageType.None, 200),
                    new Pattern.Stage(PatternStageType.LeftBrakeLight, 175),
                    new Pattern.Stage(PatternStageType.RightTailLight, 175),
                    new Pattern.Stage(PatternStageType.None, 200),
                    new Pattern.Stage(PatternStageType.LeftTailLight, 175),
                    new Pattern.Stage(PatternStageType.RightBrakeLight, 175),
                    new Pattern.Stage(PatternStageType.None, 200),
                    new Pattern.Stage(PatternStageType.LeftBrakeLight, 175),
                    new Pattern.Stage(PatternStageType.RightTailLight, 175),
                    new Pattern.Stage(PatternStageType.None, 200),
                }),

                new Pattern("Testing TailLight", new Pattern.Stage[]
                {
                    new Pattern.Stage(PatternStageType.LeftTailLight, 500),
                    new Pattern.Stage(PatternStageType.None, 500),
                    new Pattern.Stage(PatternStageType.RightTailLight, 500),
                    new Pattern.Stage(PatternStageType.None, 500),
                }),

                new Pattern("Testing BrakeLight", new Pattern.Stage[]
                {
                    new Pattern.Stage(PatternStageType.LeftBrakeLight, 500),
                    new Pattern.Stage(PatternStageType.None, 500),
                    new Pattern.Stage(PatternStageType.RightBrakeLight, 500),
                    new Pattern.Stage(PatternStageType.None, 500),
                }),

                new Pattern("Testing BrakeLight/TailLight", new Pattern.Stage[]
                {
                    new Pattern.Stage(PatternStageType.LeftBrakeLight | PatternStageType.LeftTailLight, 500),
                    new Pattern.Stage(PatternStageType.None, 500),
                    new Pattern.Stage(PatternStageType.RightBrakeLight | PatternStageType.RightTailLight, 500),
                    new Pattern.Stage(PatternStageType.None, 500),
                }),
#endif

                new Pattern("Left-Right Slow", new Pattern.Stage[]
                {
                    new Pattern.Stage(PatternStageType.LeftHeadlight | PatternStageType.LeftBrakeLight | PatternStageType.LeftTailLight, 210),
                    new Pattern.Stage(PatternStageType.BothHeadlights | PatternStageType.BothBrakeLights | PatternStageType.BothTailLights, 210),
                    new Pattern.Stage(PatternStageType.RightHeadlight | PatternStageType.RightBrakeLight | PatternStageType.RightTailLight, 210),
                    new Pattern.Stage(PatternStageType.BothHeadlights | PatternStageType.BothBrakeLights | PatternStageType.BothTailLights, 210),
                }),

                new Pattern("Left-Right Slow 2", new Pattern.Stage[]
                {
                    new Pattern.Stage(PatternStageType.LeftHeadlight | PatternStageType.LeftBrakeLight | PatternStageType.LeftTailLight, 210),
                    new Pattern.Stage(PatternStageType.None, 180),
                    new Pattern.Stage(PatternStageType.LeftHeadlight | PatternStageType.LeftBrakeLight| PatternStageType.LeftTailLight, 210),
                    new Pattern.Stage(PatternStageType.BothHeadlights | PatternStageType.BothBrakeLights | PatternStageType.BothTailLights, 260),
                    new Pattern.Stage(PatternStageType.RightHeadlight | PatternStageType.RightBrakeLight | PatternStageType.RightTailLight, 210),
                    new Pattern.Stage(PatternStageType.None, 180),
                    new Pattern.Stage(PatternStageType.RightHeadlight | PatternStageType.RightBrakeLight | PatternStageType.RightTailLight, 210),
                    new Pattern.Stage(PatternStageType.BothHeadlights | PatternStageType.BothBrakeLights | PatternStageType.BothTailLights, 260),
                }),

                new Pattern("Both Slow", new Pattern.Stage[]
                {
                    new Pattern.Stage(PatternStageType.None, 230),
                    new Pattern.Stage(PatternStageType.BothHeadlights | PatternStageType.BothBrakeLights, 230),
                    new Pattern.Stage(PatternStageType.None, 230),
                    new Pattern.Stage(PatternStageType.BothHeadlights | PatternStageType.BothTailLights, 230),
                }),

                new Pattern("Both Fast", new Pattern.Stage[]
                {
                    new Pattern.Stage(PatternStageType.None, 135),
                    new Pattern.Stage(PatternStageType.BothHeadlights| PatternStageType.BothBrakeLights, 140),
                    new Pattern.Stage(PatternStageType.None, 135),
                    new Pattern.Stage(PatternStageType.BothHeadlights | PatternStageType.BothTailLights, 140),
                }),

                new Pattern("Left-Right Fast", new Pattern.Stage[]
                {
                    new Pattern.Stage(PatternStageType.LeftHeadlight | PatternStageType.LeftBrakeLight | PatternStageType.LeftTailLight , 110),
                    new Pattern.Stage(PatternStageType.BothHeadlights | PatternStageType.BothBrakeLights | PatternStageType.BothTailLights, 110),
                    new Pattern.Stage(PatternStageType.RightHeadlight | PatternStageType.RightBrakeLight | PatternStageType.RightTailLight, 110),
                    new Pattern.Stage(PatternStageType.BothHeadlights | PatternStageType.BothBrakeLights | PatternStageType.BothTailLights, 110),
                }),

                new Pattern("Left-Right Fast 2", new Pattern.Stage[]
                {
                    new Pattern.Stage(PatternStageType.LeftHeadlight | PatternStageType.LeftBrakeLight | PatternStageType.LeftTailLight, 150),
                    new Pattern.Stage(PatternStageType.None, 115),
                    new Pattern.Stage(PatternStageType.LeftHeadlight | PatternStageType.LeftBrakeLight| PatternStageType.LeftTailLight, 150),
                    new Pattern.Stage(PatternStageType.BothHeadlights | PatternStageType.BothBrakeLights | PatternStageType.BothTailLights, 130),
                    new Pattern.Stage(PatternStageType.RightHeadlight | PatternStageType.RightBrakeLight | PatternStageType.RightTailLight, 150),
                    new Pattern.Stage(PatternStageType.None, 115),
                    new Pattern.Stage(PatternStageType.RightHeadlight | PatternStageType.RightBrakeLight | PatternStageType.RightTailLight, 150),
                    new Pattern.Stage(PatternStageType.BothHeadlights | PatternStageType.BothBrakeLights | PatternStageType.BothTailLights, 130),
                }),
            };
        }


        private const string PatternXMLExplanationText = @"<!--

  A Pattern will loop through its stages.
  Each stage specifies the lights to turn on (all the lights not specified will be turned off)
  and the time(in milliseconds) it will last.

  If one stage needs more than one light,
  each light name has to be separated by a space.

  LIGHT NAMES:
    None				    (all lights turned off)
	
	LeftHeadlight
	RightHeadlight
	
	LeftTailLight
	RightTailLight
	
	LeftBrakeLight
	RightBrakeLight
	
	BothHeadlights			(same as ""LeftHeadlight RightHeadlight"")
    BothTailLights          (same as ""LeftTailLight RightTailLight"")
    BothBrakeLights         (same as ""LeftBrakeLight RightBrakeLight"")

    All



  Example:

  <Pattern Name=""the name of this pattern"">                                 < the name displayed in the UI
    <Stages>
      <Stage>
        <Type>LeftHeadlight RightTailLight LeftBrakeLight</Type>            < this lights to turn on
        <Milliseconds>210</Milliseconds>     								< the time in milliseconds that this stage will last
      </Stage>
      <Stage>
        <Type>BothHeadlights BothBrakeLights</Type>
        <Milliseconds>210</Milliseconds>
      </Stage>
      <Stage>
        <Type>RightHeadlight LeftTailLight RightBrakeLight</Type>
        <Milliseconds>210</Milliseconds>
      </Stage>
    </Stages>
  </Pattern>

-->

";

    }
}
