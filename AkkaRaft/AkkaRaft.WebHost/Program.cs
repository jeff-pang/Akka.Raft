using Serilog;
using System;
using System.IO;

namespace AkkaRaft.WebHost
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

            AppMain app = new AppMain();
            app.Start();
        }
    }
}