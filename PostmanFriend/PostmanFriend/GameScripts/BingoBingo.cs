using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using PostmanFriend.Protocols;
using PostmanFriend.Protocols.BingoBingo;

namespace PostmanFriend.GameScripts
{
    class BingoBingo
    {
        private readonly Postman _postMan = new Postman();
        public readonly PostmanPower _postManPower = new PostmanPower();

        /// <summary>
        /// 取得機台使用狀況(Get)
        /// </summary>
        /// <returns></returns>
        public async Task<GetMachineListAPI> GetMachineList(string uri, string path, Dictionary<string, string> header = null)
        {
            GetMachineListAPI getMachineListAPI = null;

            try
            {
                string result = await _postMan.HttpGetAsync(uri, path, header);

                if (result.IndexOf("\"otherMachineList\"") != -1)
                {
                    getMachineListAPI = JsonConvert.DeserializeObject<GetMachineListAPI>(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return getMachineListAPI;
        }

        /// <summary>
        /// 進入機台
        /// </summary>
        /// <returns></returns>
        public async Task<string> EnterMachine(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            string message = "";

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"errorMsg\"") != -1)
                {
                    EnterMachineAPI enterMachineAPI = JsonConvert.DeserializeObject<EnterMachineAPI>(result);
                    message = enterMachineAPI.errorMsg;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return message;
        }

        /// <summary>
        /// 連線驗證
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectVerify(string uri)
        {
            bool success =false;
            string message;
            try
            {
                await _postManPower.Connect(uri);

                //protocol
                object command1 = new
                {
                    protocol = "json",
                    version = 1
                };
                await _postManPower.Send(command1);
                message = await _postManPower.Receive();

                success = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return success;
        }

        public async Task<float> Spin(object[] arguments)
        {
            float score = -1;
            object command = new
            {
                type = 6
            };

            object command1 = new
            {
                type = 1,
                invocationId = "4",
                nonblocking = false,
                target = "BuyTicket",
                arguments
            };

            string message;
            //下注
            if (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                await _postManPower.Send(command1);
            }

            bool getData = false;
            //接收開球資料
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData == false)
            {
                message = await _postManPower.Receive();
                List<string> messageList = StringCut(message, "\"type\"");

                for (int i = 0; i < messageList.Count; i++)
                {
                    if (messageList[i].Contains("RefreshIsOpen"))
                    {
                        getData = true;
                        break;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }

                await Task.Delay(100);
            }

            //接收資料
            getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData == false)
            {
                message = await _postManPower.Receive();
                List<string> messageList = StringCut(message, "\"type\"");

                for (int i = 0; i < messageList.Count; i++)
                {
                    if (messageList[i].Contains("GotNewDrawInfo"))
                    {
                        getData = true;
                        break;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }

                await Task.Delay(100);
            }

            //等待秒
            int second = 6;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && second > 0) {
                message = await _postManPower.Receive();
                List<string> messageList = StringCut(message, "\"type\"");

                for (int i = 0; i < messageList.Count; i++)
                {
                    if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }

                second -= 1;
                await Task.Delay(100);
            }


            object command2 = new
            {
                type = 1,
                invocationId = "4",
                nonblocking = false,
                target = "GetRedeemedTicket",
                arguments = new object[] { }
            };

            //請求得分結果
            if (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                await _postManPower.Send(command2);
            }

            //解析結果
            getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData == false)
            {
                message = await _postManPower.Receive();
                List<string> messageList = StringCut(message, "\"type\"");
                
                for (int i = 0; i < messageList.Count; i++)
                {
                    if (messageList[i].Contains("RefreshRedeemedTickets"))
                    {
                        BingoBingoRefreshRedeemedTickets bingoBingoRefreshRedeemedTickets = JsonConvert.DeserializeObject<BingoBingoRefreshRedeemedTickets>(messageList[i]);
                        string arg = bingoBingoRefreshRedeemedTickets.arguments[0];
                        List<BingoBingoRefreshRedeemedTicketsArguments> argData = JsonConvert.DeserializeObject<List<BingoBingoRefreshRedeemedTicketsArguments>>(arg);
                        
                        float currencyAmount = argData[0].CurrencyAmount;
                        float totalBet = argData[0].TotalBet;
                        float totalWin = argData[0].TotalWin;
                        score = currencyAmount / totalBet * totalWin;

                        getData = true;
                        break;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }

                await Task.Delay(100);
            }

            return score;
        }

        List<string> StringCut(string origin, string cut)
        {
            string[] strings = origin.Split(new string[] { cut }, StringSplitOptions.RemoveEmptyEntries);
            List<string> newStrings = new List<string>();
            foreach (var item in strings)
            {
                newStrings.Add(item);
            }

            if (newStrings.Count > 1)
            {
                string firstString = newStrings[0];
                newStrings.RemoveAt(0);

                for (int i = 0; i < newStrings.Count; i++)
                {
                    newStrings[i] = firstString + cut + newStrings[i];

                    int num = newStrings[i].LastIndexOf(firstString);

                    if (i != newStrings.Count - 1)
                    {
                        newStrings[i] = newStrings[i].Remove(num, firstString.Length);
                    }
                }
            }

            return newStrings;
        }

        /// <summary>
        /// 離開遊戲
        /// </summary>
        /// <returns></returns>
        public async Task LeaveGame(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            try
            {
                await _postManPower.Close();
                string result = await _postMan.HttpPostAsync(uri, path, data, header);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
