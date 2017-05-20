using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AkkaRaft.WebHost
{
    public partial class SystemConfig
    {
        static Dictionary<string, SystemConfig> _configs;

        static SystemConfig()
        {
            _configs = new Dictionary<string, SystemConfig>();
            string baseLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string machineName = Environment.MachineName;
            string[] files = Directory.GetFiles(baseLocation, $"{machineName}.*.jcfg");

            for (int x = 0; x < files.Length; x++)
            {
                SystemConfig config = new SystemConfig(files[x]);
                _configs.Add(config._name, config);
            }
        }

        public static T GetConfig<T>(string name)
        {
            if (_configs.ContainsKey(name))
            {
                return _configs[name].Cast<T>();
            }

            return default(T);
        }
    }
}
