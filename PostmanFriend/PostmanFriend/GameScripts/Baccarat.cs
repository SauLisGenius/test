using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using PostmanFriend.Protocols;
using PostmanFriend.Protocols.Baccarat;

namespace PostmanFriend.GameScripts
{
    class Baccarat
    {
        private readonly Postman _postMan = new Postman();
        public readonly PostmanPower _postManPower = new PostmanPower();

        /// <summary>
        /// 取得機台使用狀況(Get)
        /// </summary>
        /// <returns></returns>
        public async Task<List<BaccaratTablesAPI>> GetTableList(string uri, string path, Dictionary<string, string> header = null)
        {
            List<BaccaratTablesAPI> baccaratTablesAPI = null;

            try
            {
                string result = await _postMan.HttpGetAsync(uri, path, header);

                if (result.IndexOf("\"groupName\"") != -1)
                {
                    baccaratTablesAPI = JsonConvert.DeserializeObject<List<BaccaratTablesAPI>>(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return baccaratTablesAPI;
        }

        /// <summary>
        /// 連線驗證
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectVerify(string uri, string groupName)
        {
            bool success = false;
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

                //platform
                object[] arguments = new object[]
                {
                    groupName
                };
                object command2 = new
                {
                    type = 1,
                    invocationId = "2",
                    nonblocking = false,
                    target = "JoinGroup",
                    arguments
                };
                await _postManPower.Send(command2);
                await _postManPower.Receive();

                success = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return success;
        }

        public async Task<long> Spin(long[] bets)
        {
            long score = -1;
            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {
                bets
            };
            object command1 = new
            {
                type = 1,
                invocationId = "4",
                nonblocking = false,
                target = "SV_SetBet",
                arguments
            };

            string message;
            List<float> winRate = new List<float>();
            List<bool> winDistrict = new List<bool>();
            if (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                await _postManPower.Send(command);

                //先確認是否是下注時間
                bool getData = false;
                while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && !getData) {
                    message = await _postManPower.Receive();

                    List<string> messageList = StringCut(message, "\"type\"");
                    for (int i = 0; i < messageList.Count; i++)
                    {
                        if (messageList[i].Contains("timer"))
                        {
                            BaccaratCL_PhaseAPI baccaratCL_PhaseAPI = JsonConvert.DeserializeObject<BaccaratCL_PhaseAPI>(messageList[i]);
                            if (baccaratCL_PhaseAPI.arguments[0].timer > 50)
                            {
                                getData = true;
                                break;
                            }
                        }
                        else if (messageList[i].Contains("\"type\":6"))
                        {
                            await _postManPower.Send(command);
                        }
                    }

                    await Task.Delay(100);
                }

                //下注
                await _postManPower.Send(command1);

                //接收特定牌局結果
                getData = false;
                while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && !getData)
                {
                    message = await _postManPower.Receive();
                    Console.WriteLine(message);
                    List<string> messageList = StringCut(message, "\"type\"");
                    for (int i = 0; i < messageList.Count; i++)
                    {
                        if (messageList[i].Contains("winDistrict"))
                        {
                            BaccaratCL_PhaseAPI baccaratCL_PhaseAPI = JsonConvert.DeserializeObject<BaccaratCL_PhaseAPI>(messageList[i]);
                            winRate = baccaratCL_PhaseAPI.arguments[0].pokerOld.winRate;
                            winDistrict = baccaratCL_PhaseAPI.arguments[0].pokerOld.winDistrict;

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

                score = 0;
                //計算得分
                for (int i = 0; i < winDistrict.Count; i++)
                {
                    score += (long)(bets[i] * winRate[i]);
                }
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
        public async Task LeaveGame()
        {
            try
            {
                await _postManPower.Close();
                //string result = await _postMan.HttpPostAsync(uri, path, data, header);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
