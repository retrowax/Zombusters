using System.Xml.Serialization;

namespace ZombustersWindows
{
    public struct OptionsState
    {
        [XmlIgnore]
        public InputMode Player;
        public float FXLevel;
        public float MusicLevel;
        public bool FullScreenMode;
        public string locale;
    }
}
