using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace iStatServerService
{
    public static class SerializationExtensions
    {
        public static string Serialize<T>(this T obj)
        {
            var serializer = new DataContractSerializer(obj.GetType());
            using (var writer = new StringWriter())
            using (var stm = new XmlTextWriter(writer))
            {
                serializer.WriteObject(stm, obj);
                return writer.ToString();
            }
        }
        public static T Deserialize<T>(this string serialized)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using (var reader = new StringReader(serialized))
            using (var stm = new XmlTextReader(reader))
            {
                return (T)serializer.ReadObject(stm);
            }
        }
    }

    class Preferences
    {
        static Preferences instance = null;
        static readonly object padlock = new object();
        Dictionary<string, string> _preferences = new Dictionary<string,string>();
        string _path;

        Preferences()
        {
        }

        public static Preferences Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Preferences();
                    }
                    return instance;
                }
            }
        }

        public void LoadDefaults()
        {
            _preferences["pin"] = "12345";
            _preferences["port"] = "5109";
            _preferences["upnpPort"] = "5109";
            _preferences["upnpEnabled"] = "0";
        }

        public void Load(string path)
        {
            _path = path + @"\preferences.xml";
            if (File.Exists(_path))
            {
                try
                {
                    string serialized = File.ReadAllText(_path);
                    Dictionary<string, string> saved = (Dictionary<string, string>)serialized.Deserialize<Dictionary<string, string>>();
                    foreach (KeyValuePair<string, string> p in saved)
                    {
                        _preferences[p.Key] = p.Value;
                    }
                }
                catch (Exception ex)
                {
                    File.Delete(_path);
                }
            }

            if(_preferences.ContainsKey("upnpDescription") == false){
                Random random = new Random();
                int randomNumber = random.Next(1000, 99999);
                Set("upnpDescription", String.Format("{0}", randomNumber));
            }
        }

        private void Save()
        {
            lock (_preferences)
            {

                if (File.Exists(_path))
                    File.Delete(_path);

                string serialized = _preferences.Serialize();
                File.WriteAllText(_path, serialized);
            }
        }

        public string Value(string key)
        {
            return _preferences[key];
        }

        public void Set(string key, string value)
        {
            _preferences[key] = value;
            Save();
        }
    }
}
