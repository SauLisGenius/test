using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.TreasureBowl
{
    class NoBonusTimeAPI
    {
        public NoBonusTimeSVData svData;
    }

    class NoBonusTimeSVData
    {
        public List<NoBonusTimeBetAndNoBonusTimes> betAndNoBonusTimes;
    }

    class NoBonusTimeBetAndNoBonusTimes
    {
        public long bet;
        public long noBonusTimes;
    }

}
