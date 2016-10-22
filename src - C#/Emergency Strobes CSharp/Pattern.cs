namespace Emergency_Strobes
{
    // System
    using System;
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
}
