using iRecon.LogManager;
using iRecon.XMLManager;
using System.Collections.Generic;
using System.IO;

namespace iRecon.DataManager
{
    public sealed class SettingsContainer
    {
        private static SettingsContainer SingletonInstance = null;
        private static readonly object Lock = new object();

        public int i_TestImageOffset { get { return DeserializeTestIOff()[0].i_TestImageOffset; } }
        public int i_BasicImageOffset { get { return DeserializeTestIOff()[0].i_BasicImageOffset; } }
        public string s_RecoImgFolder { get { return DeserializeTestIOff()[0].s_RecoImgFolder; } }
        public string s_TestImgDir { get { return DeserializeTestIOff()[0].s_TestImgDir; } }
        public int i_K { get { return DeserializeTestIOff()[0].i_K; } }
        public double d_UniquenessThreshold { get { return DeserializeTestIOff()[0].d_UniquenessThreshold; } }

        private List<Data> DeserializeTestIOff()
        {
            ErrInfLogger.LockInstance.InfoLog("Start of the DeserializeTestIOff");

            SerializationManager<ObjectToUse> SM = new SerializationManager<ObjectToUse>(SingletonStorage.Instance.s_Path);
            ObjectToUse OTU = new ObjectToUse();
            OTU = SM.Deserialize();

            ErrInfLogger.LockInstance.InfoLog("End of the DeserializeTestIOff");
            return OTU.s_DataList;
        }
        
        public string s_Path { get { return GetPath(); } }
        public string GetPath() { return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); }

        public static SettingsContainer Instance
        {
            get
            {
                lock (Lock)
                {
                    if (SingletonInstance == null)
                    {
                        SingletonInstance = new SettingsContainer();
                    }
                    return SingletonInstance;
                }
            }
        }
    }
}
