using Akka.Configuration;
using System;
using System.Linq;
using System.IO;
using Akka.Actor;
using Serilog;

namespace lighthouse
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = AppContext.BaseDirectory;

            Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .WriteTo.LiterateConsole()
                   .WriteTo.RollingFile(path + "\\logs\\{Date}.log")
                   .CreateLogger();

            var systemName = "raftsystem";

            string ip = args[0];
            string port = args[1];

            string basePath = AppContext.BaseDirectory;
            string hocon = File.ReadAllText(Path.Combine(basePath, "akka.hocon"));
            var config = ConfigurationFactory.ParseString(hocon.Replace("$ip", ip).Replace("$port", port));

            var lighthouseConfig = config.GetConfig("lighthouse");
            if (lighthouseConfig != null)
            {
                systemName = lighthouseConfig.GetString("actorsystem", systemName);
            }
            
            var selfAddress = string.Format("akka.tcp://{0}@{1}:{2}", systemName, ip, port);
            var seeds = config.GetStringList("akka.cluster.seed-nodes");
            if (!seeds.Contains(selfAddress))
            {
                seeds.Add(selfAddress);
            }

            Log.Information("seed addresses are {0}", string.Join(",", string.Join(",", seeds.Select(s => "\"" + s + "\""))));

            var injectedClusterConfigString = "akka.cluster.seed-nodes = [" + string.Join(",", seeds.Select(s => "\"" + s + "\"")) + "]";

            var finalConfig = ConfigurationFactory.ParseString(injectedClusterConfigString)
                .WithFallback(config);

            var system = ActorSystem.Create(systemName, finalConfig);
            
            Console.ReadLine();
        }
    }
}