using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Nancy.Owin;
using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using AkkaRaft.WebHost.Websockets;

namespace AkkaRaft.WebHost
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app
            .UseWebSockets()
            .MapWebSockManager("/ws", serviceProvider.GetService<ActorStatusHandler>())
            .UseOwin(x => x.UseNancy());
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebsockManager();
        }
    }
}