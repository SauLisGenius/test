using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PostmanFriend.Protocols.BQ;
using PostmanFriend.Protocols.BQ.Response;
using System.Diagnostics;

namespace PostmanFriend.GameScripts
{
    class BQ_Data
    {
        public string GameName;//遊戲名稱
        public long TotalBet;//總押注
        public long TotalScore;//總得點
        public long TotalWin;//總勝點
        public long GoldSpan;//鑽幣變動 = 總勝點
    }

    class BQ
    {
        private readonly Postman _postMan = new Postman();

        /// <summary>
        /// BQ登入驗證
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetVerify(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            string verify = "";

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"sVerify\"") != -1)
                {
                    BQAdminLoginAPI bQAdminLoginAPI = JsonConvert.DeserializeObject<BQAdminLoginAPI>(result);
                    verify = bQAdminLoginAPI.Data.sVerify;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return verify;
        }

        /// <summary>
        /// 取得水果盤當日遊玩資料
        /// </summary>
        /// <returns></returns>
        public async Task<BQ_Data> GetFruitScore(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            BQ_Data bQ_Data = new BQ_Data();

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"longColumn\"") != -1)
                {
                    //將回傳資料去除前面的符號，我這邊從第5個字元開始取資料
                    string resultSub = result.Substring(6);

                    BQBatchedDataResponseAPI batchedDataResponseAPI = JsonConvert.DeserializeObject<BQBatchedDataResponseAPI>(resultSub);
                    bQ_Data.TotalBet = batchedDataResponseAPI.dataResponse[4].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalScore = batchedDataResponseAPI.dataResponse[3].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalWin = batchedDataResponseAPI.dataResponse[5].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bQ_Data;
        }

        /// <summary>
        /// 取得北歐諸神當日遊玩資料
        /// </summary>
        /// <returns></returns>
        public async Task<BQ_Data> GetRagnarokScore(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            BQ_Data bQ_Data = new BQ_Data();

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"longColumn\"") != -1)
                {
                    //將回傳資料去除前面的符號，我這邊從第5個字元開始取資料
                    string resultSub = result.Substring(6);

                    BQBatchedDataResponseAPI batchedDataResponseAPI = JsonConvert.DeserializeObject<BQBatchedDataResponseAPI>(resultSub);
                    bQ_Data.TotalBet = batchedDataResponseAPI.dataResponse[2].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalScore = batchedDataResponseAPI.dataResponse[3].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalWin = batchedDataResponseAPI.dataResponse[4].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bQ_Data;
        }

        /// <summary>
        /// 取得赤壁三國當日遊玩資料
        /// </summary>
        /// <returns></returns>
        public async Task<BQ_Data> GetChibiScore(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            BQ_Data bQ_Data = new BQ_Data();

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"longColumn\"") != -1)
                {
                    //將回傳資料去除前面的符號，我這邊從第5個字元開始取資料
                    string resultSub = result.Substring(6);

                    BQBatchedDataResponseAPI batchedDataResponseAPI = JsonConvert.DeserializeObject<BQBatchedDataResponseAPI>(resultSub);
                    bQ_Data.TotalBet = batchedDataResponseAPI.dataResponse[4].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalScore = batchedDataResponseAPI.dataResponse[3].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalWin = batchedDataResponseAPI.dataResponse[5].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bQ_Data;
        }

        /// <summary>
        /// 取得7PK紅當日遊玩資料
        /// </summary>
        /// <returns></returns>
        public async Task<BQ_Data> GetPK7RedScore(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            BQ_Data bQ_Data = new BQ_Data();

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"longColumn\"") != -1)
                {
                    //將回傳資料去除前面的符號，我這邊從第5個字元開始取資料
                    string resultSub = result.Substring(6);

                    BQBatchedDataResponseAPI batchedDataResponseAPI = JsonConvert.DeserializeObject<BQBatchedDataResponseAPI>(resultSub);
                    bQ_Data.TotalBet = batchedDataResponseAPI.dataResponse[2].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalScore = batchedDataResponseAPI.dataResponse[3].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalWin = batchedDataResponseAPI.dataResponse[4].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bQ_Data;
        }

        /// <summary>
        /// 取得招財喵吉當日遊玩資料
        /// </summary>
        /// <returns></returns>
        public async Task<BQ_Data> GetMiaoJiScore(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            BQ_Data bQ_Data = new BQ_Data();

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"longColumn\"") != -1)
                {
                    //將回傳資料去除前面的符號，我這邊從第5個字元開始取資料
                    string resultSub = result.Substring(6);

                    BQBatchedDataResponseAPI batchedDataResponseAPI = JsonConvert.DeserializeObject<BQBatchedDataResponseAPI>(resultSub);
                    bQ_Data.TotalBet = batchedDataResponseAPI.dataResponse[2].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalScore = batchedDataResponseAPI.dataResponse[3].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalWin = bQ_Data.TotalScore - bQ_Data.TotalBet;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bQ_Data;
        }

        /// <summary>
        /// 取得聚寶盆當日遊玩資料
        /// </summary>
        /// <returns></returns>
        public async Task<BQ_Data> GetTreasureBowlScore(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            BQ_Data bQ_Data = new BQ_Data();

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"longColumn\"") != -1)
                {
                    //將回傳資料去除前面的符號，我這邊從第5個字元開始取資料
                    string resultSub = result.Substring(6);

                    BQBatchedDataResponseAPI batchedDataResponseAPI = JsonConvert.DeserializeObject<BQBatchedDataResponseAPI>(resultSub);
                    bQ_Data.TotalBet = batchedDataResponseAPI.dataResponse[2].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalScore = batchedDataResponseAPI.dataResponse[3].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalWin = bQ_Data.TotalScore - bQ_Data.TotalBet;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bQ_Data;
        }

        /// <summary>
        /// 取得賭城狂歡當日遊玩資料
        /// </summary>
        /// <returns></returns>
        public async Task<BQ_Data> GetVegasScore(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            BQ_Data bQ_Data = new BQ_Data();

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"longColumn\"") != -1)
                {
                    //將回傳資料去除前面的符號，我這邊從第5個字元開始取資料
                    string resultSub = result.Substring(6);

                    BQBatchedDataResponseAPI batchedDataResponseAPI = JsonConvert.DeserializeObject<BQBatchedDataResponseAPI>(resultSub);
                    bQ_Data.TotalBet = batchedDataResponseAPI.dataResponse[2].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalScore = batchedDataResponseAPI.dataResponse[3].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalWin = bQ_Data.TotalScore - bQ_Data.TotalBet;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bQ_Data;
        }

        /// <summary>
        /// 取得百家樂當日遊玩資料
        /// </summary>
        /// <returns></returns>
        public async Task<BQ_Data> GetBaccaratScore(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            BQ_Data bQ_Data = new BQ_Data();

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"longColumn\"") != -1)
                {
                    //將回傳資料去除前面的符號，我這邊從第5個字元開始取資料
                    string resultSub = result.Substring(6);
                    BQBatchedDataResponseAPI batchedDataResponseAPI = JsonConvert.DeserializeObject<BQBatchedDataResponseAPI>(resultSub);

                    bQ_Data.TotalBet = batchedDataResponseAPI.dataResponse[2].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalScore = batchedDataResponseAPI.dataResponse[3].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalWin = bQ_Data.TotalScore - bQ_Data.TotalBet;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bQ_Data;
        }

        /// <summary>
        /// 取得推筒子當日遊玩資料
        /// </summary>
        /// <returns></returns>
        public async Task<BQ_Data> GetPushChessScore(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            BQ_Data bQ_Data = new BQ_Data();

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"longColumn\"") != -1)
                {
                    //將回傳資料去除前面的符號，我這邊從第5個字元開始取資料
                    string resultSub = result.Substring(6);
                    BQBatchedDataResponseAPI batchedDataResponseAPI = JsonConvert.DeserializeObject<BQBatchedDataResponseAPI>(resultSub);

                    bQ_Data.TotalBet = batchedDataResponseAPI.dataResponse[1].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalScore = batchedDataResponseAPI.dataResponse[2].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalWin = batchedDataResponseAPI.dataResponse[5].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bQ_Data;
        }

        /// <summary>
        /// 取得小瑪莉當日遊玩資料
        /// </summary>
        /// <returns></returns>
        public async Task<BQ_Data> GetFarmerMarioScore(string uri, string path, string data, Dictionary<string, string> header = null)
        {
            BQ_Data bQ_Data = new BQ_Data();

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"longColumn\"") != -1)
                {
                    //將回傳資料去除前面的符號，我這邊從第5個字元開始取資料
                    string resultSub = result.Substring(6);
                    BQBatchedDataResponseAPI batchedDataResponseAPI = JsonConvert.DeserializeObject<BQBatchedDataResponseAPI>(resultSub);

                    for (int i = 0; i < batchedDataResponseAPI.dataResponse[0].dataSubset[0].dataset.tableDataset.column[7].longColumn.values.Count; i++)
                    {
                        bQ_Data.TotalBet += batchedDataResponseAPI.dataResponse[0].dataSubset[0].dataset.tableDataset.column[3].longColumn.values[i];
                        bQ_Data.TotalScore += batchedDataResponseAPI.dataResponse[0].dataSubset[0].dataset.tableDataset.column[7].longColumn.values[i];
                        bQ_Data.TotalWin += batchedDataResponseAPI.dataResponse[0].dataSubset[0].dataset.tableDataset.column[8].longColumn.values[i];
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bQ_Data;
        }

        /// <summary>
        /// 取得BingoBingo當日遊玩資料
        /// </summary>
        /// <returns></returns>
        public async Task<BQ_Data> GetBingoBingoScore(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            BQ_Data bQ_Data = new BQ_Data();

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"doubleColumn\"") != -1)
                {
                    //將回傳資料去除前面的符號，我這邊從第5個字元開始取資料
                    string resultSub = result.Substring(6);
                    BQBatchedDataResponseAPI batchedDataResponseAPI = JsonConvert.DeserializeObject<BQBatchedDataResponseAPI>(resultSub);

                    bQ_Data.TotalBet = batchedDataResponseAPI.dataResponse[2].dataSubset[0].dataset.tableDataset.column[0].doubleColumn.values[0];
                    bQ_Data.TotalScore = batchedDataResponseAPI.dataResponse[3].dataSubset[0].dataset.tableDataset.column[0].doubleColumn.values[0];
                    bQ_Data.TotalWin = batchedDataResponseAPI.dataResponse[4].dataSubset[0].dataset.tableDataset.column[0].doubleColumn.values[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bQ_Data;
        }

        /// <summary>
        /// 取得MuseStar當日遊玩資料
        /// </summary>
        /// <returns></returns>
        public async Task<BQ_Data> GetMuseStarScore(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            BQ_Data bQ_Data = new BQ_Data();

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"longColumn\"") != -1)
                {
                    //將回傳資料去除前面的符號，我這邊從第5個字元開始取資料
                    string resultSub = result.Substring(6);
                    BQBatchedDataResponseAPI batchedDataResponseAPI = JsonConvert.DeserializeObject<BQBatchedDataResponseAPI>(resultSub);

                    bQ_Data.TotalBet = batchedDataResponseAPI.dataResponse[2].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalScore = batchedDataResponseAPI.dataResponse[3].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalWin = batchedDataResponseAPI.dataResponse[4].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bQ_Data;
        }

        /// <summary>
        /// 取得Pisces2當日遊玩資料
        /// </summary>
        /// <returns></returns>
        public async Task<BQ_Data> GetPisces2Score(string uri, string path, object data, Dictionary<string, string> header = null)
        {
            BQ_Data bQ_Data = new BQ_Data();

            try
            {
                string result = await _postMan.HttpPostAsync(uri, path, data, header);

                if (result.IndexOf("\"longColumn\"") != -1)
                {
                    //將回傳資料去除前面的符號，我這邊從第5個字元開始取資料
                    string resultSub = result.Substring(6);
                    BQBatchedDataResponseAPI batchedDataResponseAPI = JsonConvert.DeserializeObject<BQBatchedDataResponseAPI>(resultSub);

                    bQ_Data.TotalBet = batchedDataResponseAPI.dataResponse[0].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalScore = batchedDataResponseAPI.dataResponse[1].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                    bQ_Data.TotalWin = batchedDataResponseAPI.dataResponse[2].dataSubset[0].dataset.tableDataset.column[0].longColumn.values[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return bQ_Data;
        }
    }
}
