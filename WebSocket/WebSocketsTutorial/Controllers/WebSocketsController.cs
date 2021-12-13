using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebSocketsTutorial.Services;

namespace WebSocketsTutorial.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketsController : ControllerBase
    {
        private readonly ILogger<WebSocketsController> _logger;

        private readonly IHostApplicationLifetime _applicationLifetime = null;

        //private readonly IList<WebSocket> _webSockets;
        private IWebSocketManager _webSocketManager;


        public WebSocketsController(ILogger<WebSocketsController> logger, IWebSocketManager webSocketManager, IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            //_webSockets = new List<WebSocket>();
            _webSocketManager = webSocketManager;
            _applicationLifetime = applicationLifetime;
        }

        [HttpGet("/ws")]
        public async Task Get()
        {
          if (HttpContext.WebSockets.IsWebSocketRequest)
          {
              using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
              _logger.Log(LogLevel.Information, "WebSocket connection established");
              await Echo(webSocket);
          }
          else
          {
              HttpContext.Response.StatusCode = 400;
          }
        }
        
        private async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _logger.Log(LogLevel.Information, "Message received from Client");

            while (!result.CloseStatus.HasValue)
            {
                var serverMsg = Encoding.UTF8.GetBytes($"Server: Hello. You said: {Encoding.UTF8.GetString(buffer)}");
                await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message sent to Client");

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message received from Client");
                
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _logger.Log(LogLevel.Information, "WebSocket connection closed");
        }

        [HttpGet("/ws2")]
        public async Task Get2()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _logger.Log(LogLevel.Information, "WebSocket connection established");

                var buffer = new byte[1024 * 4];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                var receivedMsg = Encoding.UTF8.GetString(buffer);

                _webSocketManager.AddWebSocket(receivedMsg, webSocket);

                await WebSocketWaitLoop(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        ///  Web Socket event loop. Just sits and waits
        /// for disconnection or error to break.
        /// </summary>
        /// <param name="webSocket">The Web Socket to wait on</param>
        /// <returns></returns>
        private async Task WebSocketWaitLoop(WebSocket webSocket)
        {
            // File Watcher was started by Middleware extensions
            var buffer = new byte[1024];
            while (webSocket.State.HasFlag(WebSocketState.Open))
            {
                try
                {
                    var received =
                        await webSocket.ReceiveAsync(buffer, _applicationLifetime.ApplicationStopping);
                }
                catch
                {
                    break;
                }
            }

            _webSocketManager.TryRemove (webSocket, out byte throwAway);

            if (webSocket.State != WebSocketState.Closed &&
                webSocket.State != WebSocketState.Aborted)
            {
                try
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                        "Socket closed",
                        _applicationLifetime.ApplicationStopping);
                }
                catch
                {
                    // this may throw on shutdown and can be ignored
                }
            }

        }

        ///  Web Socket event loop. Just sits and waits
        /// for disconnection or error to break.
        /// </summary>
        /// <param name="webSocket">The Web Socket to wait on</param>
        /// <returns></returns>
        private async Task WebSocketWaitLoopByName(string callerName, WebSocket webSocket)
        {
            // File Watcher was started by Middleware extensions
            var buffer = new byte[1024];
            while (webSocket.State.HasFlag(WebSocketState.Open))
            {
                try
                {
                    var received =
                        await webSocket.ReceiveAsync(buffer, _applicationLifetime.ApplicationStopping);
                }
                catch
                {
                    break;
                }
            }

            _webSocketManager.TryRemove(callerName);

            if (webSocket.State != WebSocketState.Closed &&
                webSocket.State != WebSocketState.Aborted)
            {
                try
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                        "Socket closed",
                        _applicationLifetime.ApplicationStopping);
                }
                catch
                {
                    // this may throw on shutdown and can be ignored
                }
            }
        }


        [HttpGet("/notifySockets")]
        public async Task Notify()
        {
            var webSockets = _webSocketManager.GetWebSocketsWithName();
            string callers = String.Empty;
            foreach (var callerName in webSockets.Keys)
            {
                callers += callerName + ", ";
            }

            foreach (var ws in webSockets)
            {
                var serverMsg = Encoding.UTF8.GetBytes($"Coucou tout le monde ! The following ones are connected: {callers}");
                await ws.Value.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

    }
}
