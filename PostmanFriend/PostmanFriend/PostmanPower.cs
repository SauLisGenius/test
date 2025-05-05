using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

namespace PostmanFriend
{
    class PostmanPower
    {
        public ClientWebSocket _clientWebSocket = new ClientWebSocket();
        CancellationToken cancellation = new CancellationToken();

        /// <summary>
        /// 用 HttpGet 呼叫 WebAPI
        /// </summary>
        /// <param name="uri">Uri基底連結</param>
        /// <returns>Json字串</returns>
        public async Task Connect(string uri)
        {
            try
            {
                await _clientWebSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// 接收Websocket
        /// </summary>
        public async Task<string> Receive()
        {
            string message = "";

            ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);
            WebSocketReceiveResult receiveResult;

            using (var ms = new MemoryStream())
            {
                do
                {
                    receiveResult = await _clientWebSocket.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, receiveResult.Count);
                }
                while (!receiveResult.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);

                if (receiveResult.MessageType == WebSocketMessageType.Text) {
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            message += line.Replace("", "");
                        }
                    }
                }
            }

            return message;
        }

        /// <summary>
        /// 寄送Websocket
        /// </summary>
        /// <param name="data">命令字串</param>
        /// <returns>Json字串</returns>
        public async Task<bool> Send(object data)
        {
            bool success = false;

            try
            {
                var sendData = System.Text.Encoding.Default.GetBytes(JsonConvert.SerializeObject(data) + "");
                await _clientWebSocket.SendAsync(new ArraySegment<byte>(sendData), WebSocketMessageType.Binary, true, cancellation);

                success = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error! " + ex.ToString());
            }

            return success;
        }

        /// <summary>
        /// 結束連線
        /// </summary>
        /// <returns>Json字串</returns>
        public async Task Close()
        {
            await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Disconnect", CancellationToken.None);
            //await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnect", CancellationToken.None);
            //await _clientWebSocket.Dispose();
        }
    }
}
