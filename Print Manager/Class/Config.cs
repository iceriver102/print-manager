
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Alta.Class
{


    [Serializable]
    public class Config
    {
        public string DataFolder = "Data";
        public string CacheFolder = "Cache";
        public string UserFile = "data.xml";
        public double LeftMargin = 10;
        public double RightMargin = 10;
        public double TopMargin = 10;
        public int CountTime = 1000;
        public int TimePrint = 500;
        public int printId = 1;
        public Stretch Fill = Stretch.Fill;
        public string cacheName = "Cache.xml";
        public bool Template = true;
        public static Config Read(string file)
        {
            if (!File.Exists(file))
            {
                Write(file, new Config());
                return new Config();
            }
            else
            {
                FileInfo inf = new FileInfo(file);
                while (inf.IsFileLocked()) { Console.WriteLine("Wait..."); };
                try
                {
                    using (Stream s = File.Open(file, FileMode.Open))
                    {
                        System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(Config));
                        return (Config)reader.Deserialize(s);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetBaseException().ToString());
                    return new Config();
                }

            }
        }

        public static void Write(string file, Config overview)
        {
            if (string.IsNullOrEmpty(file))
                throw new Exception("File Not Empty");
            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(typeof(Config));
            FileInfo inf = new FileInfo(file);
            while (inf.IsFileLocked() && inf.Exists) { Console.WriteLine("Wait..."); }
            if (!inf.Exists)
            {
                using (Stream s = File.Open(file,FileMode.OpenOrCreate))
                {
                    writer.Serialize(s, overview);
                }
            }
            else
            {
                using (Stream s = File.Open(file, FileMode.Truncate))
                {
                    writer.Serialize(s, overview);
                }
            }
        }
    }
}
