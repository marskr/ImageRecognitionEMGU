using System;
using System.IO;
using System.Windows.Data;

namespace iRecon.DataManager
{
    [ValueConversion(typeof(int), typeof(string))]
    public class SimpleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int returnedValue;

            if (int.TryParse((string)value, out returnedValue))
            {
                return returnedValue;
            }

            throw new Exception("The text is not a number");
        }
    }
    public sealed class SettingsContainer
    {
        private static SettingsContainer SingletonInstance = null;
        private static readonly object Lock = new object();

        public const int i_testImageOffset = 95, i_basicImageOffset = 77;
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
