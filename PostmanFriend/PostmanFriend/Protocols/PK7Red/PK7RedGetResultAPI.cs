using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.PK7Red
{
    class PK7RedGetResultAPI
    {
        public PK7RedGetResultServerData svData;
    }

    class PK7RedGetResultServerData {
        public long iJackpot;
        public long iAwardNormalWin;
        public long iAwardBonusWin;

    }
}
