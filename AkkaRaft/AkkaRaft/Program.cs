using Akka.Actor;
using Akka.Configuration;
using System;
using System.Linq;
using DotNetty;
using System.IO;
using Akka.Routing;
using Serilog;
using System.Globalization;
using AkkaRaft.Shared;
using System.Reflection;
using AkkaRaft.Shared.Heartbeats;
using AkkaRaft.Shared.Nodes;
using System.Runtime.InteropServices;

namespace AkkaRaft
{
    class Program
    {
        static void Main(string[] args)
        {
            var assm = Assembly.GetEntryAssembly();

            string path = AppContext.BaseDirectory;

            Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .WriteTo.LiterateConsole()
                   .WriteTo.RollingFile(path + "\\logs\\{Date}.log")
                   .CreateLogger();

            string basePath = AppContext.BaseDirectory;
            string hocon = File.ReadAllText(Path.Combine(basePath, "akka.hocon"));  
            
            var config = ConfigurationFactory.ParseString(hocon);

            using (var system = ActorSystem.Create("raftsystem", config))
            {
                var builder = new NodeFactory();
                var node=builder.MakeNode(system);
                Log.Information("Program started. 'exit' to shutdown");

                string input = null;

                do
                {
                    input=Console.ReadLine();
                } while (input?.ToLower() != "exit");
            }
        }        
    }
}