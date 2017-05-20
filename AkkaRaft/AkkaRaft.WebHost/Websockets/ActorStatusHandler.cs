using Serilog;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace AkkaRaft.WebHost.Websockets
{
    public class ActorStatusHandler : WebsockHandler
    {
        public ActorStatusHandler(WebsockManager mgr) : base(mgr)
        {   
        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);
            var socketId = WebsocketManager.GetId(socket);
            Log.Information("{socketId} is now connected", socketId);
        }

        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}