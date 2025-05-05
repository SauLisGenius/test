using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using PostmanFriend.Protocols;
using PostmanFriend.Protocols.Vegas;

namespace PostmanFriend.GameScripts
{
    class Vegas
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
        /// 連線驗證
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectVerify(string uri, long zoneType, long machineNo)
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

                if (message.Contains("OnEnterSuccessful"))
                {
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return success;
        }

        /// <summary>
        /// 取得機台(是否有青再連線)資料
        /// </summary>
        /// <returns></returns>
        public async Task<long> GetMachineInfo()
        {
            long Award = -1;
            try
            {
                //GetMachineInfo
                object arguments = new object[] { };
                object command = new
                {
                    type = 1,
                    invocationId = "3",
                    nonblocking = false,
                    target = "GetMachineInfo",
                    arguments
                };

                if (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    await _postManPower.Send(command);
                }

                bool getData = false;
                string message = "";
                while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && !getData)
                {
                    message = await _postManPower.Receive();
                    if (message.Contains("BetAndAward"))
                    {
                        VegasStartGameAPI vegasStartGameAPI = JsonConvert.DeserializeObject<VegasStartGameAPI>(message.Replace("", ""));
                        string result = vegasStartGameAPI.result;
                        VegasStartGameResult2 vegasStartGameResult2 = JsonConvert.DeserializeObject<VegasStartGameResult2>(result);
                        Award = vegasStartGameResult2.BetAndAward[0].Award;//[0] = 下注額300，[1] = 下注額600，[2] = 下注額900
                        Console.WriteLine("Award " + Award);

                        getData = true;
                        break;
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return Award;
        }

        public async Task<long> Spin(long bet)
        {
            long score = -1;

            object[] arguments = new object[]
            {
                bet
            };
            object command1 = new
            {
                type = 1,
                invocationId = "4",
                nonblocking = false,
                target = "Spin",
                arguments
            };

            string message = "";
            if (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                await _postManPower.Send(command1);

                message = await _postManPower.Receive();
                Console.WriteLine("message = " + message);
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            bool getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && !getData)
            {
                if (message.Contains("NoBonusTimes") && message.Contains("Reward"))
                {
                    VegasStartGameAPI vegasStartGameAPI = JsonConvert.DeserializeObject<VegasStartGameAPI>(message.Replace("", ""));
                    string result = vegasStartGameAPI.result;
                    VegasStartGameResult vegasStartGameResult = JsonConvert.DeserializeObject<VegasStartGameResult>(result);

                    //一般
                    score = vegasStartGameResult.AwardStateObject.Reward;

                    //Bonus
                    if (vegasStartGameResult.AwardStateObject.Bonus != null)
                    {
                        long bonusScore = 0;
                        foreach (var item in vegasStartGameResult.AwardStateObject.Bonus)
                        {
                            bonusScore += item.Reward;
                        }

                        score += bonusScore;
                    }

                    //外送得分
                    if (vegasStartGameResult.Grab7Event.Current == 0 && vegasStartGameResult.AwardStateObject.Bonus != null)
                    {
                        long extra = vegasStartGameResult.Grab7Event.Bet / 3 * vegasStartGameResult.Grab7Event.Reward;//一枚硬幣的價值*200 = 外送得分
                        score += extra;
                    }

                    //判斷是否為青再連線
                    if (score == 0)
                    {
                        if (vegasStartGameResult.AwardStateObject.Award == 4)
                        {
                            score = 4;//用4當作代號，表示青再連線，下一局下注免費
                        }
                    }

                    TimeSpan ts = stopwatch.Elapsed;
                    if (ts.Seconds > 3) { break; }

                    getData = true;
                }

                await Task.Delay(100);
            }

            return score;
        }

        /// <summary>
        /// 離開遊戲
        /// </summary>
        /// <returns></returns>
        public async Task Leave()
        {
            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {
                false, false
            };
            object command1 = new
            {
                type = 1,
                invocationId = "4",
                nonblocking = false,
                target = "Leave",
                arguments
            };

            await _postManPower.Send(command1);

            string message;
            bool getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData != true)
            {
                message = await _postManPower.Receive();

                List<string> messageList = StringCut(message, "{\"type\"");
                for (int i = 0; i < messageList.Count; i++)
                {
                    Console.WriteLine("messageList[i]" + messageList[i]);
                    if (messageList[i].Contains("TransferCode"))
                    {
                        getData = true;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }

                await Task.Delay(100);
            }

        }

        List<string> StringCut(string origin, string cut)
        {
            string[] strings = origin.Split(new string[] { cut }, StringSplitOptions.RemoveEmptyEntries);//移除切割後
            List<string> newStrings = new List<string>();
            foreach (var item in strings)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    newStrings.Add(item);
                }
            }

            for (int i = 0; i < newStrings.Count; i++)
            {
                newStrings[i] = cut + newStrings[i];
            }

            return newStrings;
        }

        /// <summary>
        /// 離開遊戲
        /// </summary>
        /// <returns></returns>
        public async Task LeaveGame(string uri, string path, object data, Dictionary<string, string> header)
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



        /// <summary>
        /// 掃BB
        /// </summary>
        /// <returns></returns>
        public async Task<VegasStartGameResult2> GetMachineInfoAward()
        {
            VegasStartGameResult2 vegasStartGameResult2 = new VegasStartGameResult2();
            try
            {
                //GetMachineInfo
                object arguments = new object[] { };
                object command = new
                {
                    type = 1,
                    invocationId = "3",
                    nonblocking = false,
                    target = "GetMachineInfo",
                    arguments
                };

                if (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    await _postManPower.Send(command);
                }

                bool getData = false;
                string message = "";
                while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && !getData)
                {
                    message = await _postManPower.Receive();
                    if (message.Contains("BetAndAward"))
                    {
                        VegasStartGameAPI vegasStartGameAPI = JsonConvert.DeserializeObject<VegasStartGameAPI>(message.Replace("", ""));
                        string result = vegasStartGameAPI.result;
                        vegasStartGameResult2 = JsonConvert.DeserializeObject<VegasStartGameResult2>(result);
                        //Award = miaoJiStartGameResult2.BetAndAward[0].Award;//[0] = 下注額300，[1] = 下注額600，[2] = 下注額900
                        //Console.WriteLine("Award " + Award);

                        getData = true;
                        break;
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return vegasStartGameResult2;
        }
    }
}
