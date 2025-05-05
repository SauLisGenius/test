using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.Vegas
{
    class VegasStartGameAPI
    {
        public long type;
        public string invocationId;
        public string result;
    }

    class VegasStartGameResult
    {
        public long PlayerBalance;
        public long NoBonusTimes;
        public VegasStartGameGrab7Event Grab7Event;
        public long AwardState;
        public VegasStartGameAwardStateObject AwardStateObject;
    }

    class VegasStartGameResult2
    {
        public long PlayerBalance;
        public List<VegasStartGameBetAndAward> BetAndAward;
        public long NoBonusTimes;
        public List<VegasStartGameGrab7Event> Grab7Event;
        public long AwardState;
        public VegasStartGameAwardStateObject AwardStateObject;
    }

    class VegasStartGameBetAndAward
    {
        public long Bet;
        public long Award;
    }

    class VegasStartGameGrab7Event
    {
        public long Bet;
        public long Current;
        public long Target;
        public long Reward;
    }

    class VegasStartGameAwardStateObject
    {
        public List<VegasStartGameBonus> Bonus;
        public long Award;
        public long Reward;
        public List<long> Line;
        public List<long> Reel1IconOrder;
        public List<long> Reel2IconOrder;
        public List<long> Reel3IconOrder;
    }

    class VegasStartGameBonus
    {
        public long Award;
        public long Reward;
        public List<long> Line;
    }
}
