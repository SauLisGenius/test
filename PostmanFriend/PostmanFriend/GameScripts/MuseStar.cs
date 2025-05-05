using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using PostmanFriend.Protocols;
using System.Threading;
using PostmanFriend.Protocols.MuseStar;

namespace PostmanFriend.GameScripts
{
    class MuseStar
    {
        private readonly Postman _postMan = new Postman();
        public readonly PostmanPower _postManPower = new PostmanPower();

        /// <summary>
        /// 連線驗證
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectVerify(string uri, long machineNo)
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
                    2,
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
                Console.WriteLine("2 = " + message);

                //GetMachineInfo
                arguments = new object[] { };
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
        /// 玩一把
        /// </summary>
        /// <returns></returns>
        public async Task<long> Spin(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            long score = -1;

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);
                Console.WriteLine(result);
                if (result.IndexOf("\"win\"") != -1)
                {
                    MuseStarStartGameAPI museStarStartGameAPI = JsonConvert.DeserializeObject<MuseStarStartGameAPI>(result);

                    long freeGameScore = 0;
                    long bonusGameScore = 0;
                    long normalScore = museStarStartGameAPI.svData.normal.win;

                    if (museStarStartGameAPI.svData.freeGame != null)
                    {
                        for (int i = 0; i < museStarStartGameAPI.svData.freeGame.Count; i++)
                        {
                            freeGameScore += museStarStartGameAPI.svData.freeGame[i].win;
                        }
                    }

                    if (museStarStartGameAPI.svData.bonusGame != null)
                    {
                        for (int i = 0; i < museStarStartGameAPI.svData.bonusGame.firstStage.Count; i++)
                        {
                            bonusGameScore += museStarStartGameAPI.svData.bonusGame.firstStage[i].win;
                        }

                        for (int i = 0; i < museStarStartGameAPI.svData.bonusGame.secondStage.Count; i++)
                        {
                            bonusGameScore += museStarStartGameAPI.svData.bonusGame.secondStage[i].win;
                        }

                        for (int i = 0; i < museStarStartGameAPI.svData.bonusGame.thirdStage.Count; i++)
                        {
                            bonusGameScore += museStarStartGameAPI.svData.bonusGame.thirdStage[i].win;
                        }
                    }

                    score = normalScore + freeGameScore + bonusGameScore;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return score;
        }

        /// <summary>
        /// 離開遊戲
        /// </summary>
        /// <returns></returns>
        public async Task LeaveGame(string uri, string path, object data, Dictionary<string, string> header)
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
