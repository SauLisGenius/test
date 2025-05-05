using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.Pisces2
{
    //取得機台是否被佔用
    class GetLobbyMachinesHighAPI
    {
        public int machineNo;
        public int minBet;
        public int maxBet;
        public int status;//1 = 閒置，2 = 佔用
    }
}
