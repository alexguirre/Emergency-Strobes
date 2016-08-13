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
            public int Ticks;

            public Stage(PatternStageType type, int ticks)
            {
                Type = type;
                Ticks = ticks;
            }
        }
    }

    public enum PatternStageType
    {
        None,
        Both,
        LeftOnly,
        RightOnly,
    }
}
