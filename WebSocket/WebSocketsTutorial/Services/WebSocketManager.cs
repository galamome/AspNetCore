using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebSocketsTutorial.Services
{
    public class WebSocketManager : IWebSocketManager
    {
        //private IList<WebSocket> _webSockets;

        /// <summary>
        /// Concurrent dictionary as a Hashset. The byte is just a throwaway value
        /// </summary>
        internal static ConcurrentDictionary<WebSocket, byte> _webSockets { get; } = new ConcurrentDictionary<WebSocket, byte>();

        /// <summary>
        /// Concurrent dictionary as a Hashset. The byte is just a throwaway value
        /// </summary>
        internal static ConcurrentDictionary<string, WebSocket> _webSocketByNames { get; } = new ConcurrentDictionary<string, WebSocket>();

        public WebSocketManager()
        {
            //_webSockets = new List<WebSocket>();
        }

        void IWebSocketManager.AddWebSocket(WebSocket webSocket)
        {
            if (!_webSockets.ContainsKey(webSocket))
            {
                _webSockets.TryAdd(webSocket, 0);
            }
        }

        ConcurrentDictionary<WebSocket, byte> IWebSocketManager.GetWebSockets()
        {
            return _webSockets;
        }

        void IWebSocketManager.TryRemove(WebSocket webSocket, out byte throwAway)
        {
            throw new NotImplementedException();
        }

        void IWebSocketManager.AddWebSocket(string callerName, WebSocket webSocket)
        {
            if (!_webSocketByNames.ContainsKey(callerName))
            {
                _webSocketByNames.TryAdd(callerName, webSocket);
            }
        }

        void IWebSocketManager.TryRemove(string callerName)
        {
            if (_webSocketByNames.ContainsKey(callerName))
            {
                WebSocket ws;
                _webSocketByNames.TryRemove(callerName, out ws);
            }
        }

        ConcurrentDictionary<string, WebSocket> IWebSocketManager.GetWebSocketsWithName()
        {
            return _webSocketByNames;
        }
    }
}
