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
    class TexasPoker
    {
        private readonly Postman _postMan = new Postman();
        public readonly PostmanPower _postManPower = new PostmanPower();

        /// <summary>
        /// 連線驗證
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectVerify(string uri)
        {
            bool success = false;
            string message = "";
            try
            {
                await _postManPower.Connect(uri);
                message = await _postManPower.Receive();
                Console.WriteLine(message);

                string base64 = "8wYBAAEBaQBysJo=";

                string utf8 = System.Text.Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(base64));
                Console.WriteLine("utf8 = " + utf8);


                await _postManPower.Send(utf8);
                message = await _postManPower.Receive();
                Console.WriteLine(message);


                success = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return success;
        }

        public async Task<float> Spin(string bets)
        {
            float score = -1;
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

            string message = "";
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
                        if (messageList[i].Contains("dicChu"))
                        {
                            //PushChessCL_GameStatusAPI pushChessCL_GameStatusAPI = JsonConvert.DeserializeObject<PushChessCL_GameStatusAPI>(messageList[i]);
                            //PushChessCL_GameStatusArgumentsA pushChessCL_GameStatusArgumentsA = JsonConvert.DeserializeObject<PushChessCL_GameStatusArgumentsA>(pushChessCL_GameStatusAPI.arguments[0]);

                            //if (pushChessCL_GameStatusArgumentsA.objData.eStatus == 2 && pushChessCL_GameStatusArgumentsA.objData.iTimer > 5)
                            //{
                            //    getData = true;
                            //    break;
                            //}
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
                bool isChuWin = false;
                bool isChuanWin = false;
                bool isWeiWin = false;
                while (_postManPower._clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open && !getData)
                {
                    message = await _postManPower.Receive();
                    List<string> messageList = StringCut(message, "\"type\"");

                    for (int i = 0; i < messageList.Count; i++)
                    {
                        if (messageList[i].Contains("dicRoad"))
                        {
                            //PushChessCL_GameStatusAPI pushChessCL_GameStatusAPI = JsonConvert.DeserializeObject<PushChessCL_GameStatusAPI>(messageList[i]);
                            //PushChessCL_GameStatusArgumentsB pushChessCL_GameStatusArgumentsB = JsonConvert.DeserializeObject<PushChessCL_GameStatusArgumentsB>(pushChessCL_GameStatusAPI.arguments[0]);
                            //PushChessCL_GameStatusDicRoad pushChessCL_GameStatusDicRoad = pushChessCL_GameStatusArgumentsB.objData.objData.dicRoad;
                            //Console.WriteLine(messageList[i]);

                            //if (pushChessCL_GameStatusDicRoad.num4 != null)
                            //{
                            //    isChuWin = pushChessCL_GameStatusDicRoad.num4.isChuWin;
                            //    isChuanWin = pushChessCL_GameStatusDicRoad.num4.isChuanWin;
                            //    isWeiWin = pushChessCL_GameStatusDicRoad.num4.isWeiWin;
                            //}
                            //else if (pushChessCL_GameStatusDicRoad.num3 != null)
                            //{
                            //    isChuWin = pushChessCL_GameStatusDicRoad.num3.isChuWin;
                            //    isChuanWin = pushChessCL_GameStatusDicRoad.num3.isChuanWin;
                            //    isWeiWin = pushChessCL_GameStatusDicRoad.num3.isWeiWin;
                            //}
                            //else if (pushChessCL_GameStatusDicRoad.num2 != null)
                            //{
                            //    isChuWin = pushChessCL_GameStatusDicRoad.num2.isChuWin;
                            //    isChuanWin = pushChessCL_GameStatusDicRoad.num2.isChuanWin;
                            //    isWeiWin = pushChessCL_GameStatusDicRoad.num2.isWeiWin;
                            //}
                            //else if (pushChessCL_GameStatusDicRoad.num1 != null)
                            //{
                            //    isChuWin = pushChessCL_GameStatusDicRoad.num1.isChuWin;
                            //    isChuanWin = pushChessCL_GameStatusDicRoad.num1.isChuanWin;
                            //    isWeiWin = pushChessCL_GameStatusDicRoad.num1.isWeiWin;
                            //}
                            //else if (pushChessCL_GameStatusDicRoad.num0 != null)
                            //{
                            //    isChuWin = pushChessCL_GameStatusDicRoad.num0.isChuWin;
                            //    isChuanWin = pushChessCL_GameStatusDicRoad.num0.isChuanWin;
                            //    isWeiWin = pushChessCL_GameStatusDicRoad.num0.isWeiWin;
                            //}

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

            if (newStrings.Count == 1)
            {
                newStrings.Clear();
                newStrings.Add(origin);
            }
            else if (newStrings.Count == 2)
            {
                newStrings.Clear();
                newStrings.Add(origin);
            }
            else if (newStrings.Count > 2)
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
                //await _postManPower.Close();
                string result = await _postMan.HttpPostAsync(uri, path, data, header);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
