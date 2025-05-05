using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using PostmanFriend.Protocols;
using PostmanFriend.Protocols.PushChess;

namespace PostmanFriend.GameScripts
{
    class PushChess
    {
        private readonly Postman _postMan = new Postman();
        public readonly PostmanPower _postManPower = new PostmanPower();

        /// <summary>
        /// 連線驗證
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectVerify(string uri, long zoneType)
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

                //log
                object[] arguments = new object[]
                {
                    "Connection Type:[UNITY_WEBGL ], Tracefl=80f176,h=www.cloudflare.com,ip=60.248.128.174,ts=1655706655.32,visit_scheme=https,uag=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Safari/537.36,colo=TPE,http=http/2,loc=TW,tls=TLSv1.3,sni=plaintext,warp=off,gateway=off,"
                };
                object command2 = new
                {
                    type = 1,
                    invocationId = "2",
                    nonblocking = false,
                    target = "SV_Log",
                    arguments
                };
                await _postManPower.Send(command2);
                await _postManPower.Receive();

                //JoinLobby
                arguments = new object[]
                {
                    zoneType
                };
                command2 = new
                {
                    type = 1,
                    invocationId = "3",
                    nonblocking = false,
                    target = "SV_JoinLobby",
                    arguments
                };
                await _postManPower.Send(command2);
                await _postManPower.Receive();

                //JoinGroup
                arguments = new object[]
                {
                    2
                };
                command2 = new
                {
                    type = 1,
                    invocationId = "4",
                    nonblocking = false,
                    target = "SV_JoinGroup",
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

        public async Task<long> Spin(string bets)
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
                invocationId = "5",
                nonblocking = false,
                target = "SV_PlayerBet",
                arguments
            };

            string message;
            //List<float> winRate = new List<float>();
            //List<bool> winDistrict = new List<bool>();
            if (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                await _postManPower.Send(command);

                //先確認是否是下注時間
                bool getData = false;
                while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && !getData)
                {
                    message = await _postManPower.Receive();
                    List<string> messageList = StringCut(message, "\"type\"");

                    for (int i = 0; i < messageList.Count; i++)
                    {
                        Console.WriteLine(messageList[i]);
                        if (messageList[i].Contains("CL_GameStatus"))
                        {
                            PushChessCL_GameStatusAPI pushChessCL_GameStatusAPI = JsonConvert.DeserializeObject<PushChessCL_GameStatusAPI>(messageList[i]);
                            PushChessCL_GameStatusArguments pushChessCL_GameStatusArguments = JsonConvert.DeserializeObject<PushChessCL_GameStatusArguments>(Convert.ToString(pushChessCL_GameStatusAPI.arguments[0]));
                            
                            if (pushChessCL_GameStatusArguments.objData.eStatus == 2 && pushChessCL_GameStatusArguments.objData.iTimer > 5)
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
                Console.WriteLine("Bet!!!");

                //接收特定牌局結果
                getData = false;
                bool isChuWin = false;
                bool isChuanWin = false;
                bool isWeiWin = false;
                while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && !getData)
                {
                    message = await _postManPower.Receive();
                    List<string> messageList = StringCut(message, "\"type\"");
                    
                    for (int i = 0; i < messageList.Count; i++)
                    {
                        Console.WriteLine(messageList[i]);
                        if (messageList[i].Contains("pdaRecord"))
                        {
                            PushChessCL_GameStatusAPI pushChessCL_GameStatusAPI = JsonConvert.DeserializeObject<PushChessCL_GameStatusAPI>(messageList[i]);
                            PushChessCL_GameStatusArguments pushChessCL_GameStatusArguments = JsonConvert.DeserializeObject<PushChessCL_GameStatusArguments>(Convert.ToString(pushChessCL_GameStatusAPI.arguments[0]));
                            
                            isChuWin = pushChessCL_GameStatusArguments.objData.objData.pdaRecord.isChuWin;
                            isChuanWin = pushChessCL_GameStatusArguments.objData.objData.pdaRecord.isChuanWin;
                            isWeiWin = pushChessCL_GameStatusArguments.objData.objData.pdaRecord.isWeiWin;

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

                //計算得分
                PushChessBets pushChessBets = JsonConvert.DeserializeObject<PushChessBets>(bets);
                score = pushChessBets.Chu * 2 * (isChuWin == true ? 1 : 0)
                        + pushChessBets.Chuan * 2 * (isChuanWin == true ? 1 : 0)
                        + pushChessBets.Wei * 2 * (isWeiWin == true ? 1 : 0);
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
