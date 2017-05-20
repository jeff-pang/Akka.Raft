using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AkkaRaft.WebHost.Websockets
{
    public static class WebsockExtensions
    {
        public static IApplicationBuilder MapWebSockManager(this IApplicationBuilder app,
                                                                 PathString path,
                                                                 WebsockHandler handler)
        {
            WebsockController.Instance.SetHandler(handler);
            return app.Map(path, a => a.UseMiddleware<WebsocketMiddleware>(handler));
        }

        public static IServiceCollection AddWebsockManager(this IServiceCollection services)
        {
            services.AddTransient<WebsockManager>();

            foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
            {
                if (type.GetTypeInfo().BaseType == typeof(WebsockHandler))
                {
                    services.AddSingleton(type);
                }
            }

            return services;
        }
    }
}