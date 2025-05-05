using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.Chibi
{
    class ChibiStartGameAPI
    {
        public ChibiStartGameServerData svData;
    }

    class ChibiStartGameServerData {
        public long iMoneyOld;
        public long iMoneyNow;

        public List<ChibiStartGameLsRound> lsRound;
    }

    class ChibiStartGameLsRound {
        public long iWinPoint;
    }
}
