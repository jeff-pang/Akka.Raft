using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AkkaRaft.WebHost.Websockets
{
    public class WebsockController
    {
        private WebsockHandler _handler;
        private static WebsockController _instance;
        public static WebsockController Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new WebsockController();
                }
                return _instance;
            }
        }

        private WebsockController()
        {
        }

        internal void SetHandler(WebsockHandler handler)
        {
            _handler = handler;
        }

        public async Task SendMessageAsync(string message)
        {
            await _handler.SendMessageToAllAsync(message);
        }
    }
}
