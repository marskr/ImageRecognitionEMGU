using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace iRecon.XMLManager
{
    public sealed class SingletonStorage
    {
        private static SingletonStorage SingletonInstance = null;
        private static readonly object Lock = new object();
        private static string s_path = GetPath();
        private static string GetPath()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\xml_files\file.xml";
        }
        public string s_Path
        {
            get { return s_path; }
        }
        public static SingletonStorage Instance
        {
            get
            {
                lock (Lock)
                {
                    if (SingletonInstance == null)
                    {
                        SingletonInstance = new SingletonStorage();
                    }
                    return SingletonInstance;
                }
            }
        }
    }
    abstract public class Manager
    {
        abstract public void Serialize();
    }
    public class SerializationManager<T> where T : class
    {
        private string s_fileName;
        public SerializationManager(string _fileName)
        {
            s_fileName = _fileName;
        }
        public void Serialize(T serializeObject)
        {
            using (FileStream file = new FileStream(s_fileName, FileMode.Create))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                serializer.Serialize(file, serializeObject);
            }
        }
        public T Deserialize()
        {
            T deserializeObject = null;
            try
            {
                using (FileStream file = new FileStream(s_fileName, FileMode.Open))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(T));
                    deserializeObject = deserializer.Deserialize(file) as T;
                }
            }
            catch (FileNotFoundException exception)
            {
                Console.WriteLine(exception.Message);
            }

            return deserializeObject;
        }
        public void SerializeIntoFile()
        {
            SerializationManager<ObjectToUse> SM = new SerializationManager<ObjectToUse>(SingletonStorage.Instance.s_Path);
            ObjectToUse OTU = new ObjectToUse();
            SM.CleaningTheFile();

            OTU.s_DataList.Add(AddToList(95, 77, @"\reco_settings\img\", @"\test\", 2, 0.8));

            SM.Serialize(OTU);
        }
        private void CleaningTheFile()
        {
            System.IO.File.WriteAllText(SingletonStorage.Instance.s_Path, string.Empty);
        }
        private Data AddToList(int i_testImageOffset, int i_basicImageOffset, string s_recoImgFolder, string s_testImgDir, int i_k,
                               double d_uniquenessThreshold)
        {
            Data DATA = new Data();
            DATA.i_TestImageOffset = i_testImageOffset;
            DATA.i_BasicImageOffset = i_basicImageOffset;
            DATA.s_RecoImgFolder = s_recoImgFolder;
            DATA.s_TestImgDir = s_testImgDir;
            DATA.i_K = i_k;
            DATA.d_UniquenessThreshold = d_uniquenessThreshold;
            return DATA;
        }
    }
    [Serializable()]
    public class ObjectToUse
    {
        public ObjectToUse()
        {
            s_DataList = new List<Data>();
        }
        public List<Data> s_DataList;
    }
    public class Data
    {
        public int i_TestImageOffset { get; set; }
        public int i_BasicImageOffset { get; set; } 
        public string s_RecoImgFolder { get; set; }
        public string s_TestImgDir { get; set; }
        public int i_K { get; set; }
        public double d_UniquenessThreshold { get; set; }
        public override string ToString()
        {
            return $"First element is {i_TestImageOffset} and the second element is {i_BasicImageOffset}";
        }
    }
}
