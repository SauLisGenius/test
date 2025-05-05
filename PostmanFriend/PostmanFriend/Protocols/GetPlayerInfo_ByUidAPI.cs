using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols
{
    class GetPlayerInfo_ByUidAPI
    {
        public bool Succ;

        public PlayerInformation Data;
    }

    class PlayerInformation
    {
        public long iPoints;
    }
}
