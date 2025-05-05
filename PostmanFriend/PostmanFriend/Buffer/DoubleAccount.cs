using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace PostmanFriend.Buffer
{
    class DoubleAccount
    {
        private readonly Postman _postMan = new Postman();

        /// <summary>
        /// 取得驗證碼
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetVerify(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            string message = "";

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);
                Console.WriteLine(result);

                if (result.IndexOf("\"Data\"") != -1)
                {
                    AdminLoginAPI adminLoginAPI = JsonConvert.DeserializeObject<AdminLoginAPI>(result);
                    message = adminLoginAPI.Data.sVerify;

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return message;
        }


        /// <summary>
        /// 取得會員資料並檢查是否有重複資料
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetMemberList(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            string message = "";
            List<string> AllPlayerName = new List<string>();
            Console.WriteLine(header);
            
            try
            {
                string result = await HttpPostAsync(uri, path, data, header);
                Console.WriteLine(result);

                if (result.IndexOf("\"Data\"") != -1)
                {
                    Console.WriteLine("OK Start");

                    GetMemberListAPI getMemberListAPI = JsonConvert.DeserializeObject<GetMemberListAPI>(result);
                    foreach (var item in getMemberListAPI.Data)
                    {
                        AllPlayerName.Add(item.nickname);
                    }

                    for (int i = 0; i < AllPlayerName.Count; i++)
                    {
                        string theName = AllPlayerName[i];
                        AllPlayerName.RemoveAt(i);

                        if (AllPlayerName.Contains(theName)) {
                            Console.WriteLine("重複的玩家帳號：" + theName);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return message;
        }


        public async Task<string> HttpPostAsync(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(uri);

            SetHeader(httpClient.DefaultRequestHeaders, header);
            HttpCookie httpCookie = new HttpCookie("Cookie");
            httpCookie.Value = "_ga=GA1.3.638961135.1658284146;_gid=GA1.3.149469444.1658883773;Login=iUid=33";

            HttpContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(path, content);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private void SetHeader(HttpRequestHeaders requestHeader, Dictionary<string, string> header)
        {
            if (header != null)
            {
                foreach (var item in header)
                {
                    requestHeader.Add(item.Key, item.Value);
                }
            }
        }

    }
}
