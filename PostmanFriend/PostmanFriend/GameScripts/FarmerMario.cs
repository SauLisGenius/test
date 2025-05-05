using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using PostmanFriend.Protocols;
using PostmanFriend.Protocols.FarmerMario;

namespace PostmanFriend.GameScripts
{
    class FarmerMario
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
        public async Task<bool> ConnectVerify(string uri, long zoneType, long machineNo)
        {
            bool success = false;
            string message = "";
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

                //Enter
                object[] arguments = new object[]
                {
                    zoneType,
                    machineNo,
                    "WebGLPlayer",
                    "1",
                    null
                };
                object command2 = new
                {
                    type = 1,
                    invocationId = "2",
                    nonblocking = false,
                    target = "Enter",
                    arguments
                };
                await _postManPower.Send(command2);
                message = await _postManPower.Receive();

                //GetMachineInfo
                arguments = new object[]
                {
                    
                };
                object command3 = new
                {
                    type = 1,
                    invocationId = "3",
                    nonblocking = false,
                    target = "GetMachineInfo",
                    arguments
                };
                await _postManPower.Send(command3);
                message = await _postManPower.Receive();

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
                target = "StartRound",
                arguments
            };

            string message = "";

            //下注
            if (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                await _postManPower.Send(command1);
            }

            bool getData = false;
            //接收特定牌局結果
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData == false)
            {
                message = await _postManPower.Receive();
                List<string> messageList = StringCut(message, "\"type\"");

                for (int i = 0; i < messageList.Count; i++)
                {
                    if (messageList[i].Contains("BoardReward"))
                    {
                        FarmerMarioStartGameAPI farmerMarioStartGameAPI = JsonConvert.DeserializeObject<FarmerMarioStartGameAPI>(messageList[i]);
                        string result = farmerMarioStartGameAPI.result;
                        FarmerMarioStartGameResult farmerMarioStartGameResult = JsonConvert.DeserializeObject<FarmerMarioStartGameResult>(result);
                        score = farmerMarioStartGameResult.BoardReward + farmerMarioStartGameResult.SlotReward;

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

        //List<string> StringCut(string origin, string cut)
        //{
        //    string[] strings = origin.Split(new string[] { cut }, StringSplitOptions.RemoveEmptyEntries);
        //    List<string> newStrings = new List<string>();
        //    foreach (var item in strings)
        //    {
        //        newStrings.Add(item);
        //    }

        //    if (newStrings.Count == 1)
        //    {
        //        newStrings.Clear();
        //        newStrings.Add(origin);
        //    }
        //    else if (newStrings.Count == 2)
        //    {
        //        newStrings.Clear();
        //        newStrings.Add(origin);
        //    }
        //    else if (newStrings.Count > 2)
        //    {
        //        string firstString = newStrings[0];
        //        newStrings.RemoveAt(0);

        //        for (int i = 0; i < newStrings.Count; i++)
        //        {
        //            newStrings[i] = firstString + cut + newStrings[i];

        //            int num = newStrings[i].LastIndexOf(firstString);

        //            if (i != newStrings.Count - 1)
        //            {
        //                newStrings[i] = newStrings[i].Remove(num, firstString.Length);
        //            }
        //        }
        //    }

        //    return newStrings;
        //}

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
