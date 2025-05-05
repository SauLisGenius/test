using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend
{
    class Postman
    {
        /// <summary>
        /// 用 HttpGet 呼叫 WebAPI
        /// </summary>
        /// <param name="uri">Uri基底連結</param>
        /// <param name="path">API路徑</param>
        /// <param name="header">Header資料</param>
        /// <returns>Json字串</returns>
        public async Task<string> HttpGetAsync(string uri, string path, Dictionary<string, string> header = null)
        {
            // Uri
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(uri);

            SetHeader(httpClient.DefaultRequestHeaders, header);

            // Send
            HttpResponseMessage response = await httpClient.GetAsync(uri + path);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }


        /// <summary>
        /// 用 HttpPost 呼叫 WebAPI
        /// </summary>
        /// <param name="uri">Uri基底連結</param>
        /// <param name="path">API路徑</param>
        /// <param name="data">傳遞的資料</param>
        /// <returns>Json字串</returns>
        public async Task<string> HttpPostAsync(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(uri);

            SetHeader(httpClient.DefaultRequestHeaders, header);

            HttpContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(path, content);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> HttpPostAsync(string uri, string path, string data, Dictionary<string, string> header = null)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(uri);

            SetHeader(httpClient.DefaultRequestHeaders, header);

            HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");

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
