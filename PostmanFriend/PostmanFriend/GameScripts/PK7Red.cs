using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using PostmanFriend.Protocols;
using PostmanFriend.Protocols.PK7Red;

namespace PostmanFriend.GameScripts
{
    class PK7Red
    {
        private readonly Postman _postMan = new Postman();

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
        public async Task<bool> EnterMachine(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            bool success;

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"errorMsg\"") != -1)
                {
                    EnterMachineAPI enterMachineAPI = JsonConvert.DeserializeObject<EnterMachineAPI>(result);
                    success = true;
                }
                else {
                    success = false;
                }
            }
            catch
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// 第一把
        /// </summary>
        /// <returns></returns>
        public async Task<long> FirstSpin(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            long id = -1;

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"svData\"") != -1)
                {
                    PK7RedStartGameAPI pK7RedStartGameAPI = JsonConvert.DeserializeObject<PK7RedStartGameAPI>(result);
                    id = pK7RedStartGameAPI.svData.id;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return id;
        }

        /// <summary>
        /// 加注
        /// </summary>
        /// <returns></returns>
        public async Task<long> AddSpin(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            long phase = -1;

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"phase\"") != -1)
                {
                    PK7RedAddBetAPI pK7RedAddBetAPI = JsonConvert.DeserializeObject<PK7RedAddBetAPI>(result);

                    phase = pK7RedAddBetAPI.svData.phase;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return phase;
        }

        /// <summary>
        /// 接收回傳
        /// </summary>
        /// <returns></returns>
        public async Task<long> GetResult(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            long score = -1;

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"iJackpot\"") != -1 && result.IndexOf("\"iAwardNormalWin\"") != -1 && result.IndexOf("\"iAwardBonusWin\"") != -1)
                {
                    long sum = 0;
                    PK7RedGetResultAPI pK7RedGetResultAPI = JsonConvert.DeserializeObject<PK7RedGetResultAPI>(result);
                    sum = pK7RedGetResultAPI.svData.iJackpot + pK7RedGetResultAPI.svData.iAwardNormalWin + pK7RedGetResultAPI.svData.iAwardBonusWin;

                    if (sum >= 0) {
                        score = sum;
                    }
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
        public async Task<string> LeaveGame(string uri, string path, Dictionary<string, string> header = null)
        {
            string message = "";

            try
            {
                string result = await _postMan.HttpGetAsync(uri, path, header);
                message = result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return message;
        }
    }
}
