using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.Baccarat
{
    class BaccaratCL_PhaseAPI
    {
        public string target;
        public List<BaccaratCL_PhaseArguments> arguments;
    }

    class BaccaratCL_PhaseArguments
    {
        public long gameStep;
        public string gameNo;
        public long timer;
        public long gameRound;
        public long gameCardCount;
        public long maxRound;
        public BaccaratCL_PhasePokerOld pokerOld;
    }

    class BaccaratCL_PhasePokerOld
    {
        public List<long> cardPoints;
        public List<float> winRate;
        public List<bool> winDistrict;
        public long thisruncount;
    }
}
