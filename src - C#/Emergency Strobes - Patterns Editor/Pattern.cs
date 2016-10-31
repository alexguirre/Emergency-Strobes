﻿namespace EmergencyStrobesPatternsEditor
{
    // System
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Xml.Serialization;

    [Serializable]
    public struct Pattern
    {
        [XmlAttribute]
        public string Name;
        public Stage[] Stages;

        public Pattern(string name, Stage[] stages)
        {
            Name = name;
            Stages = stages;
        }

        public override string ToString()
        {
            return Name;
        }

        [Serializable]
        public struct Stage
        {
            public PatternStageType Type;
            public uint Milliseconds;

            public Stage(PatternStageType type, uint milliseconds)
            {
                Type = type;
                Milliseconds = milliseconds;
            }
        }
    }

    [Flags]
    public enum PatternStageType
    {
        None = 0,

        LeftHeadlight = 1 << 1,
        RightHeadlight = 1 << 2,

        LeftTailLight = 1 << 3,
        RightTailLight = 1 << 4,

        LeftBrakeLight = 1 << 5,
        RightBrakeLight = 1 << 6,

        BothHeadlights = LeftHeadlight | RightHeadlight,
        BothTailLights = LeftTailLight | RightTailLight,
        BothBrakeLights = LeftBrakeLight | RightBrakeLight,

        All = BothHeadlights | BothTailLights | BothBrakeLights,
    }



    internal class PatternWrapper : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Pattern pattern;
        public Pattern Pattern { get { return pattern; } }

        public string Name
        {
            get { return Pattern.Name; }
            set
            {
                if (value == Pattern.Name)
                    return;
                pattern.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public Pattern.Stage[] Stages
        {
            get { return Pattern.Stages; }
            set
            {
                if (value == Pattern.Stages)
                    return;
                pattern.Stages = value;
                OnPropertyChanged(nameof(Stages));
            }
        }

        public PatternWrapper(Pattern pattern)
        {
            this.pattern = pattern;
        }

        public PatternWrapper(string name, Pattern.Stage[] stages)
        {
            this.pattern = new Pattern(name, stages);
        }

        public override string ToString()
        {
            return Name;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



    internal static class PatternsIO
    {
        public static void SaveTo(string fileName, Pattern[] patterns)
        {
            string xmlText = "";
            using (StringWriter writer = new StringWriter())
            {
                XmlSerializer s = new XmlSerializer(typeof(Pattern[]));
                s.Serialize(writer, patterns);
                xmlText = writer.ToString().Replace("\"?>", "\"?>" + Environment.NewLine + PatternXMLExplanationText); // add the explanation comment after the XML declaration
            }

            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                writer.Write(xmlText);
            }
        }

        public static Pattern[] LoadFrom(string fileName)
        {
            Pattern[] patterns = null;
            XmlSerializer s = new XmlSerializer(typeof(Pattern[]));
            using (StreamReader reader = new StreamReader(fileName))
            {
                patterns = (Pattern[])s.Deserialize(reader);
            }
            return patterns;
        } 

        private const string PatternXMLExplanationText = @"<!--

  FILE GENERATED BY Emergency Strobes - Patterns Editor by alexguirre

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
