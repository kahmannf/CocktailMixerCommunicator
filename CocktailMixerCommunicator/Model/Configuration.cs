using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CocktailMixerCommunicator.Model
{
    public class Configuration
    {
        public string CMStateDirectory { get; set; }
        public string COMPort { get; set; }
        public int BaudRate { get; set; }


        public static void SaveToPath(string path, Configuration config)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Configuration));

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(fs, config);
            }
        }
        

        public static Configuration LoadFromPath(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Configuration));

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                return (Configuration)serializer.Deserialize(fs);
            }
        }
    }
}
