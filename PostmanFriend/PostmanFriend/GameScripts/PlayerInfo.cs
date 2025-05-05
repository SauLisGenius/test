using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using PostmanFriend.Protocols;

namespace PostmanFriend.GameScripts
{
    class PlayerInfo
    {
        private readonly Postman _postMan = new Postman();

        /// <summary>
        /// 取得uid、token
        /// </summary>
        /// <returns></returns>
        public async Task<AuthenticatePasswordFormatMd5API> GetAuthenticate(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            AuthenticatePasswordFormatMd5API authenticatePasswordFormatMd5 = null;

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data);
                Console.WriteLine(result);
                if (result.IndexOf("\"token\"") != -1)
                {
                    authenticatePasswordFormatMd5 = JsonConvert.DeserializeObject<AuthenticatePasswordFormatMd5API>(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return authenticatePasswordFormatMd5;
        }

        /// <summary>
        /// 取得玩家資訊
        /// </summary>
        /// <returns></returns>
        public async Task<GetPlayerInfo_ByUidAPI> GetPlayerInfo(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            GetPlayerInfo_ByUidAPI getPlayerInfo_ByUid = null;

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"iPoints\"") != -1)
                {
                    getPlayerInfo_ByUid = JsonConvert.DeserializeObject<GetPlayerInfo_ByUidAPI>(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return getPlayerInfo_ByUid;
        }


    }
}
