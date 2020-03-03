using System.Collections.Generic;
using System.Xml.Serialization;
using Yoloanno.Types;

namespace Yoloanno.Config
{    
    public class UIConfig
    {
        [XmlArray]
        public List<RegionSize> RegionTemplates;

        public string FileSystemPath;
        public string LastFileNumber;

        public UIConfig()
        {
            RegionTemplates = new List<RegionSize>();
        }

        [XmlIgnore]
        public static UIConfig Instance
        {
            get
            {
                var config = SerializationProcessor.Deserialize<UIConfig>(Path);
                if(config == null)
                {
                    config = new UIConfig();
                }
                return config;
            }
        }

        public void Save()
        {
            SerializationProcessor.Serialize(Path, this);
        }

        public static string Path
        {
            get
            {
                return System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.xml");
            }
        }
    }
}
