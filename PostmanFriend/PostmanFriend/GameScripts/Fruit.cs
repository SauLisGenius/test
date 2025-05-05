using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using PostmanFriend.Protocols;
using PostmanFriend.Protocols.Fruit;

namespace PostmanFriend
{
    class Fruit : GameLinkInterface
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
                else
                {
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
        /// 玩一把
        /// </summary>
        /// <returns></returns>
        public async Task<long> Spin(string uri, string path, object data, Dictionary<string, string> header = null) {
            long score = -1;

            try {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"iTotalWin\"") != -1)
                {
                    FruitStartGameAPI startGameAPI = JsonConvert.DeserializeObject<FruitStartGameAPI>(result);
                    score = startGameAPI.svData.iTotalWin;
                }
            } 
            catch(Exception ex) 
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


        public async Task<GetMachineListAPI> GetMachineInfo(string uri, string path, Dictionary<string, string> header = null)
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
    }
}
