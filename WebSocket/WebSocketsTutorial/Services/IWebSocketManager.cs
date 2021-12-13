using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebSocketsTutorial.Services
{
    public interface IWebSocketManager
    {
        public void AddWebSocket(WebSocket webSocket);

        public void AddWebSocket(string callerName, WebSocket webSocket);

        public void TryRemove(WebSocket webSocket, out byte throwAway);

        public void TryRemove(string callerName);
        public ConcurrentDictionary<WebSocket, byte> GetWebSockets();

        public ConcurrentDictionary<string, WebSocket> GetWebSocketsWithName();
    }
}
