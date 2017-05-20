using Nancy;
using Nancy.Extensions;
using Nancy.IO;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AkkaRaft.WebHost.WebApi
{
    public class UserPageModule:NancyModule
    {
        public UserPageModule()
        {
            Get("/Status", p =>{

                return View["Views/Status.html"];
            });

            Post("/Node/Start", p => {
                
                Log.Information("Received Node Start");
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    Arguments = @"start dotnet Y:\Github\Akka.Raft\AkkaRaft\AkkaRaft\bin\Debug\netcoreapp1.0\AkkaRaft.dll"
                };

                var proc = new Process { StartInfo = startInfo };
                proc.Start();
                                
                return HttpStatusCode.OK;
            });

            Post("Node/Terminate", p => {

                string procId = RequestStream.FromStream(Request.Body).AsString();

                if (!string.IsNullOrEmpty(procId))
                {
                    if(int.TryParse(procId,out int processId))
                    {
                        Process process = Process.GetProcessById(processId);
                        
                        if (process != null)
                        {
                            process.Kill();
                        }
                    }
                }

                return HttpStatusCode.OK;
            });
        }
    }
}