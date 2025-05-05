using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PostmanFriend.Protocols.Pisces2;
using System.Diagnostics;

namespace PostmanFriend.GameScripts
{
    class Pisces2
    {
        private readonly Postman _postMan = new Postman();
        public readonly PostmanPower _postManPower = new PostmanPower();

        /// <summary>
        /// 取得機台使用狀況(Get)
        /// </summary>
        /// <returns></returns>
        public async Task<List<GetLobbyMachinesHighAPI>> GetMachineList(string uri, string path, Dictionary<string, string> header = null)
        {
            List<GetLobbyMachinesHighAPI> getLobbyMachinesHighAPI = null;

            try
            {
                string result = await _postMan.HttpGetAsync(uri, path, header);
                Console.WriteLine(result);
                if (result.IndexOf("\"machineNo\"") != -1)
                {
                    getLobbyMachinesHighAPI = JsonConvert.DeserializeObject<List<GetLobbyMachinesHighAPI>>(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return getLobbyMachinesHighAPI;
        }

        /// <summary>
        /// 連線驗證
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectVerify(string uri, int zoneType, long machineNo)
        {
            bool success = false;
            string message = "";
            try
            {
                await _postManPower.Connect(uri);
                Console.WriteLine("machineNo = " + machineNo);
                //protocol
                object command1 = new
                {
                    protocol = "json",
                    version = 1
                };
                await _postManPower.Send(command1);
                message = await _postManPower.Receive();
                Console.WriteLine("1 = " + message);

                //platform
                object[] arguments = new object[]
                {
                    zoneType,
                    machineNo
                };
                object command2 = new
                {
                    type = 1,
                    invocationId = "1",
                    target = "JoinMachine",
                    arguments
                };
                await _postManPower.Send(command2);
                message = await _postManPower.Receive();
                Console.WriteLine("2 = " + message);

                //GetMachineInfo
                arguments = new object[] { };
                object command3 = new
                {
                    type = 1,
                    invocationId = "2",
                    target = "InitMachine",
                    arguments
                };
                await _postManPower.Send(command3);
                message = await _postManPower.Receive();
                Console.WriteLine("3 = " + message);

                message = await _postManPower.Receive();
                Console.WriteLine("4 = " + message);

                success = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return success;
        }

        /// <summary>
        /// 取得機台資訊
        /// </summary>
        /// <returns></returns>
        public async Task<long> InitMachine()
        {
            long credits = 0;
            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {

            };
            object command1 = new
            {
                type = 1,
                invocationId = "2",
                target = "InitMachine",
                arguments
            };

            await _postManPower.Send(command1);

            //確認Credit是否兌換成功(是否有鑽幣)
            bool getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData != true)
            {
                string message = await _postManPower.Receive();

                List<string> messageList = StringCut(message, "{\"type\"");
                for (int i = 0; i < messageList.Count; i++)
                {
                    Console.WriteLine(messageList[i]);
                    if (messageList[i].Contains("RefreshCurrentCredits"))
                    {
                        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                        credits = long.Parse( pisces2StartGameAPI.arguments[0].ToString());
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

            return credits;
        }

        /// <summary>
        /// 自開
        /// </summary>
        /// <returns></returns>
        public async Task<int> AddCredits()
        {
            int credits = 0;
            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {
                
            };
            object command1 = new
            {
                type = 1,
                invocationId = "3",
                target = "AddCredits",
                arguments
            };

            await _postManPower.Send(command1);

            //確認Credit是否兌換成功(是否有鑽幣)
            bool getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData != true)
            {
                string message = await _postManPower.Receive();

                List<string> messageList = StringCut(message, "{\"type\"");
                for (int i = 0; i < messageList.Count; i++)
                {
                    Console.WriteLine(messageList[i]);
                    if (messageList[i].Contains("RefreshCurrentBalance"))
                    {
                        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                    }
                    else if (messageList[i].Contains("RefreshExchangeInfo"))
                    {
                        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                        Pisces2StartGamePlayerInfo pisces2StartGamePlayerInfo = JsonConvert.DeserializeObject<Pisces2StartGamePlayerInfo>(pisces2StartGameAPI.arguments[0].ToString());
                        credits = pisces2StartGamePlayerInfo.creditCount;
                        getData = true;
                        break;
                    }
                    else if (messageList[i].Contains("error"))//身上鑽幣不夠
                    {
                        credits = 0;
                        break;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }

                await Task.Delay(100);
            }

            return credits;
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
        /// 押注
        /// </summary>
        /// <returns></returns>
        public async Task<int> Bet(long bet)
        {
            int alreadyBet = -1;//下注額
            int remainderCredits = -1;//玩家身上剩餘鑽幣
            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {
                bet
            };
            object command1 = new
            {
                type = 1,
                invocationId = "4",
                target = "Bet",
                arguments
            };

            await _postManPower.Send(command1);

            bool getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData != true)
            {
                string message = await _postManPower.Receive();
                List<string> messageList = StringCut(message, "{\"type\"");
                for (int i = 0; i < messageList.Count; i++)
                {
                    Console.WriteLine(messageList[i]);
                    if (messageList[i].Contains("RefreshCurrentBet"))
                    {
                        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                        alreadyBet = Convert.ToInt32(pisces2StartGameAPI.arguments[0]);
                    }
                    else if (messageList[i].Contains("RefreshCurrentCredits"))
                    {
                        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                        remainderCredits = Convert.ToInt32(pisces2StartGameAPI.arguments[0]);

                        getData = true;
                        break;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }
            }

            return remainderCredits;
        }

        /// <summary>
        /// 押滿注
        /// </summary>
        /// <returns></returns>
        public async Task<int> BetAll(long bet)
        {
            int alreadyBet = -1;//下注額
            int remainderCredits = -1;//玩家身上剩餘鑽幣
            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {
                
            };
            object command1 = new
            {
                type = 1,
                invocationId = "3",
                target = "BetAll",
                arguments
            };

            await _postManPower.Send(command1);

            bool getData = false;
            //while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData != true)
            //{
            //    string message = await _postManPower.Receive();

                //List<string> messageList = StringCut(message, "{\"type\"");
                //for (int i = 0; i < messageList.Count; i++)
                //{
                //    if (messageList[i].Contains("RefreshCurrentBet"))
                //    {
                //        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                //        alreadyBet = (int)pisces2StartGameAPI.arguments[0];
                //    }
                //    else if (messageList[i].Contains("RefreshCurrentCredits"))
                //    {
                //        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                //        remainderCredits = (int)pisces2StartGameAPI.arguments[0];

                //        getData = true;
                //        break;
                //    }
                //    else if (messageList[i].Contains("\"type\":6"))
                //    {
                //        await _postManPower.Send(command);
                //    }
                //}
            //}

            return remainderCredits;
        }

        /// <summary>
        /// 開始遊戲
        /// </summary>
        /// <returns></returns>
        public async Task<string> StartGame(long bet)
        {
            string round = "";//局號
            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {
                
            };
            object command1 = new
            {
                type = 1,
                invocationId = "4",
                target = "StartGame",
                arguments
            };

            await _postManPower.Send(command1);

            bool getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData != true)
            {
                string message = await _postManPower.Receive();

                List<string> messageList = StringCut(message, "{\"type\"");
                for (int i = 0; i < messageList.Count; i++)
                {
                    if (messageList[i].Contains("RefreshGameNo"))
                    {
                        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                        round = pisces2StartGameAPI.arguments[0].ToString();
                        getData = true;
                        break;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }
            }

            return round;
        }

        /// <summary>
        /// 開牌
        /// </summary>
        /// <returns></returns>
        public async Task<Pisces2StartGameBetResponse> DealCard(long bet)
        {
            Pisces2StartGameBetResponse pisces2StartGameBetResponse = new Pisces2StartGameBetResponse();//開牌內容
            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {
                
            };
            object command1 = new
            {
                type = 1,
                invocationId = "4",
                target = "DealCard",
                arguments
            };

            await _postManPower.Send(command1);

            bool getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData != true)
            {
                string message = await _postManPower.Receive();

                List<string> messageList = StringCut(message, "{\"type\"");
                for (int i = 0; i < messageList.Count; i++)
                {
                    Console.WriteLine(messageList[i]);
                    if (messageList[i].Contains("RefreshPokerCards"))
                    {
                        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                        pisces2StartGameBetResponse = JsonConvert.DeserializeObject<Pisces2StartGameBetResponse>(pisces2StartGameAPI.arguments[0].ToString());

                        getData = true;
                        break;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }
            }

            return pisces2StartGameBetResponse;
        }

        /// <summary>
        /// 全開牌
        /// </summary>
        /// <returns></returns>
        public async Task<Pisces2StartGameBetResponse> DealAll(long bet)
        {
            Pisces2StartGameBetResponse pisces2StartGameBetResponse = new Pisces2StartGameBetResponse();//開牌內容
            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {
                
            };
            object command1 = new
            {
                type = 1,
                invocationId = "4",
                target = "DealAll",
                arguments
            };

            await _postManPower.Send(command1);

            bool getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData != true)
            {
                string message = await _postManPower.Receive();

                List<string> messageList = StringCut(message, "{\"type\"");
                for (int i = 0; i < messageList.Count; i++)
                {
                    Console.WriteLine(messageList[i]);
                    if (messageList[i].Contains("RefreshPokerCards"))
                    {
                        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                        pisces2StartGameBetResponse = JsonConvert.DeserializeObject<Pisces2StartGameBetResponse>(pisces2StartGameAPI.arguments[0].ToString());

                        getData = true;
                        break;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }
            }

            return pisces2StartGameBetResponse;
        }


        public void PokerTypeJudge(List<Pisces2StartGamePokerInfo> pokers)
        {
            string pokerType = "高牌(含有非皇家2pair)";
            Console.WriteLine(pokers.Count);
            //2個對子，其中一對是JQKA
            int a = pokers.GroupBy(x => x.id % 13).Where(g => g.Count() == 1).Count();
            Console.WriteLine("a = " + a);
            //3張相同

            //順子，數字相連

            //5張花色相同

            //7張花色相同，不含鬼牌

            //3張相同+2張相同

            //3張相同+2張相同，皆為JQKA，不含鬼牌

            //4張相同

            //5張花色相同，數字相連

            //5張相同(4張相同+1鬼牌，3張相同+2鬼牌)

            //5張同花色10 JQKA (可含鬼牌)

        }


        /// <summary>
        /// 比倍
        /// </summary>
        /// <returns></returns>
        public async Task<long> DoubleUp(bool guessBig)//true = 猜大，false = 猜小
        {
            Pisces2StartGameRefreshDoubleUpResult pisces2StartGameRefreshDoubleUpResult = new Pisces2StartGameRefreshDoubleUpResult();
            long totalWin = 0;

            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {
                guessBig
            };
            object command1 = new
            {
                type = 1,
                invocationId = "4",
                target = "DoubleUp",
                arguments
            };

            await _postManPower.Send(command1);

            bool getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData != true)
            {
                string message = await _postManPower.Receive();

                List<string> messageList = StringCut(message, "{\"type\"");
                for (int i = 0; i < messageList.Count; i++)
                {
                    Console.WriteLine(messageList[i]);
                    if (messageList[i].Contains("RefreshDoubleUpResult"))
                    {
                        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                        pisces2StartGameRefreshDoubleUpResult = JsonConvert.DeserializeObject<Pisces2StartGameRefreshDoubleUpResult>(pisces2StartGameAPI.arguments[0].ToString());
                        totalWin = pisces2StartGameRefreshDoubleUpResult.totalWin;

                        getData = true;
                        break;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }
            }

            return totalWin;
        }

        /// <summary>
        /// 勝負結果
        /// </summary>
        /// <returns></returns>
        public async Task<Pisces2StartGameAwardResult> AwardResult()
        {
            Pisces2StartGameAwardResult pisces2StartGameAwardResult = new Pisces2StartGameAwardResult();

            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {
                
            };
            object command1 = new
            {
                type = 1,
                invocationId = "4",
                target = "AwardResult",
                arguments
            };

            await _postManPower.Send(command1);

            bool getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData != true)
            {
                string message = await _postManPower.Receive();

                List<string> messageList = StringCut(message, "{\"type\"");
                for (int i = 0; i < messageList.Count; i++)
                {
                    Console.WriteLine(messageList[i]);
                    if (messageList[i].Contains("RefreshAwardInfo"))
                    {
                        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                        pisces2StartGameAwardResult = JsonConvert.DeserializeObject<Pisces2StartGameAwardResult>(pisces2StartGameAPI.arguments[0].ToString());

                        getData = true;
                        break;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }
            }

            return pisces2StartGameAwardResult;
        }

        /// <summary>
        /// 得分
        /// </summary>
        /// <returns></returns>
        public async Task<Pisces2StartGameRefreshEventInfo> GameResult()
        {
            Pisces2StartGameRefreshEventInfo pisces2StartGameRefreshEventInfo = new Pisces2StartGameRefreshEventInfo();

            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {

            };
            object command1 = new
            {
                type = 1,
                invocationId = "4",
                target = "GameResult",
                arguments
            };

            await _postManPower.Send(command1);

            bool getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData != true)
            {
                string message = await _postManPower.Receive();

                List<string> messageList = StringCut(message, "{\"type\"");
                for (int i = 0; i < messageList.Count; i++)
                {
                    Console.WriteLine(messageList[i]);
                    if (messageList[i].Contains("RefreshEventInfo"))
                    {
                        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                        pisces2StartGameRefreshEventInfo = JsonConvert.DeserializeObject<Pisces2StartGameRefreshEventInfo>(pisces2StartGameAPI.arguments[0].ToString());

                        getData = true;
                        break;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }
            }

            return pisces2StartGameRefreshEventInfo;
        }

        /// <summary>
        /// 自洗
        /// </summary>
        /// <returns></returns>
        public async Task<long> TakeCredits()
        {
            Pisces2StartGameRefreshExchangeInfo pisces2StartGameRefreshExchangeInfo = new Pisces2StartGameRefreshExchangeInfo();
            long credits = 0;

            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {

            };
            object command1 = new
            {
                type = 1,
                invocationId = "4",
                target = "TakeCredits",
                arguments
            };

            await _postManPower.Send(command1);

            bool getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData != true)
            {
                string message = await _postManPower.Receive();

                List<string> messageList = StringCut(message, "{\"type\"");
                for (int i = 0; i < messageList.Count; i++)
                {
                    Console.WriteLine(messageList[i]);
                    if (messageList[i].Contains("RefreshExchangeInfo"))
                    {
                        Pisces2StartGameAPI pisces2StartGameAPI = JsonConvert.DeserializeObject<Pisces2StartGameAPI>(messageList[i]);
                        pisces2StartGameRefreshExchangeInfo = JsonConvert.DeserializeObject<Pisces2StartGameRefreshExchangeInfo>(pisces2StartGameAPI.arguments[0].ToString());

                        credits = pisces2StartGameRefreshExchangeInfo.creditCount;

                        getData = true;
                        break;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }
            }

            return credits;
        }

        /// <summary>
        /// 離開機台
        /// </summary>
        /// <returns></returns>
        public async Task LeaveMachine()
        {
            object command = new
            {
                type = 6
            };

            object[] arguments = new object[]
            {

            };
            object command1 = new
            {
                type = 1,
                invocationId = "4",
                target = "LeaveMachine",
                arguments
            };

            await _postManPower.Send(command1);

            bool getData = false;
            while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && getData != true)
            {
                string message = await _postManPower.Receive();

                List<string> messageList = StringCut(message, "{\"type\"");
                for (int i = 0; i < messageList.Count; i++)
                {
                    Console.WriteLine(messageList[i]);
                    if (messageList[i].Contains("result"))
                    {
                        getData = true;
                        break;
                    }
                    else if (messageList[i].Contains("\"type\":6"))
                    {
                        await _postManPower.Send(command);
                    }
                }
            }
        }
    }
}
