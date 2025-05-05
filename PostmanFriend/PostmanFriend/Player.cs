using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace PostmanFriend
{
    class ActiveInfo {
        public string id;
        public string pwd;
        public string apiGatewayUnity;
        public int zoneType;
        public string gameName;
        public int myMachineNo;
    }

    class Player
    {
        Thread t1;

        /// <summary>
        /// 用 HttpGet 呼叫 WebAPI
        /// </summary>
        /// <param name="uri">Uri基底連結</param>
        /// <param name="path">API路徑</param>
        /// <param name="header">Header資料</param>
        /// <returns>Json字串</returns>
        public void Play(ActiveInfo activeInfo)
        {
            t1 = new Thread(() => PlayGame(activeInfo));
            t1.IsBackground = true;
            t1.Start();
        }

        //while嘗試進入機台，3分鐘無法進入，關閉線程
        //進入後遊玩N場
        //離開機台
        void PlayGame(ActiveInfo activeInfo)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            TimeSpan ts = stopwatch.Elapsed;

            while (ts.Seconds < 180)
            {




                ts = stopwatch.Elapsed;

                Thread.Sleep(1000);
            }
        }

        
    }
}
