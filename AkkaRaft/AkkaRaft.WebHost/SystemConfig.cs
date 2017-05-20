using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AkkaRaft.WebHost
{
    public partial class SystemConfig
    {
        string _name;
        JObject _jObject;
        JArray _jArray;
        private SystemConfig(string filePath)
        {
            string[] fileName = Path.GetFileNameWithoutExtension(filePath).Split('.');
            _name = fileName[1];
            string json = File.ReadAllText(filePath);


            json = json.Trim(' ', '\n', '\r');
            if (json.StartsWith("{"))
            {
                _jObject = JObject.Parse(json);
            }
            else if (json.StartsWith("["))
            {
                _jArray = JArray.Parse(json);
            }
        }

        public T Cast<T>()
        {
            if (_jObject != null)
            {
                return _jObject.ToObject<T>();
            }
            else if (_jArray != null)
            {
                return _jArray.ToObject<T>();
            }
            else
            {
                return default(T);
            }
        }
    }
}
