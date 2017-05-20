using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Akka.Configuration;
using Akka.Actor;
using AkkaRaft.WebHost.Actors;

namespace AkkaRaft.WebHost
{
    public class AppMain
    {
        public class ServiceConfig
        {
            public string[] baseUris { get; set; }
        }

        public void Start()
        {
            if (JsonConvert.DefaultSettings == null)
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    StringEscapeHandling = StringEscapeHandling.EscapeHtml
                };
            }

            string basePath = AppContext.BaseDirectory;
            string hocon = File.ReadAllText(Path.Combine(basePath, "akka.hocon"));
            var akkaconfig = ConfigurationFactory.ParseString(hocon);

            var system = ActorSystem.Create("raftsystem", akkaconfig);            
            var statusActor = system.ActorOf<StatusActor>("status");

            var webconfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "hosting.jcfg"), false)
                .Build();

            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}