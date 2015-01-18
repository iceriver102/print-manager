using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alta.Class;

namespace Print_Manager.Class
{
    public class FileDatas
    {
        public List<FileData> Datas;
        public int Count;
        
        
        public FileDatas()
        {
            Datas = new List<FileData>();
            Count = 0;
        }
        public void Add(FileData data)
        {
            Datas.Add(data);
            Count++;
            FileDatas.Write(App.setting.cacheName, this);
        }

        public void Remove(FileData f)
        {
            Datas.Remove(f);
            Count = Datas.Count;
            FileDatas.Write(App.setting.cacheName, this);
        }

        public void Print(FileData data)
        {
            for(int i = 0; i < this.Count; i++)
            {
                if(data.Name.CompareTo(this.Datas[i].Name)==0)
                {
                    this.Datas[i].isPrint = true;
                    FileDatas.Write(App.setting.cacheName, this);
                    return;
                }
            }
        }
        public bool Optimize()
        {
            this.Datas = this.Datas.Where(item => File.Exists(item.FullName)).ToList<FileData>();
            if(this.Count != Datas.Count)
            {
                this.Count = this.Datas.Count;
                FileDatas.Write(App.setting.cacheName, this);
                return false;
            }
            return true;
            
        }
        public FileData get(int i = 0)
        {
            this.Optimize();
            if (i >= 0 && i < Count)
                return this.Datas[i];
            else
                return null;
        }

        public static FileDatas Read(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new Exception("Ten file khong dc trong");
            FileInfo inf = new FileInfo(fileName);
            if (inf.Exists)
            {
                while (inf.IsFileLocked()) { Console.WriteLine("Wait..."); };
                try
                {
                    using (Stream s = File.Open(fileName, FileMode.Open))
                    {
                        System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(FileDatas));
                        return (FileDatas)reader.Deserialize(s);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetBaseException().ToString());
                    return new FileDatas();
                }
            }else
            {
                Write(fileName, new FileDatas());
                return new FileDatas();
            }
        }

        public static void Write(string file, FileDatas overview)
        {
            if (string.IsNullOrEmpty(file))
                throw new Exception("File Not Empty");
            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(typeof(FileDatas));
            FileInfo inf = new FileInfo(file);
            while (inf.IsFileLocked() && inf.Exists) { Console.WriteLine("Wait..."); }
            if (!inf.Exists)
            {
                using (Stream s = File.Open(file, FileMode.OpenOrCreate))
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
