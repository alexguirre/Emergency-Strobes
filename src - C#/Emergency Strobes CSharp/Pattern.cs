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
            public int Milliseconds;

            public Stage(PatternStageType type, int milliseconds)
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
        BothHeadlights = LeftHeadlight | RightHeadlight,
    }
}
