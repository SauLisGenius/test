using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.TreasureBowl
{
    class TreasureBowlStartGameAPI
    {
        public long type;
        public string invocationId;
        public string result;
    }

    class TreasureBowlStartGameResult
    {
        public long PlayerBalance;
        public long NoBonusTimes;
        public TreasureBowlStartGameGrab7Event Grab7Event;
        public long AwardState;
        public TreasureBowlStartGameAwardStateObject AwardStateObject;
    }

    class TreasureBowlStartGameResult2
    {
        public long PlayerBalance;
        public List<TreasureBowlStartGameBetAndAward> BetAndAward;
        public long NoBonusTimes;
        public List<TreasureBowlStartGameGrab7Event> Grab7Event;
        public long AwardState;
        public TreasureBowlStartGameAwardStateObject AwardStateObject;
    }

    class TreasureBowlStartGameBetAndAward
    {
        public long Bet;
        public long Award;
    }

    class TreasureBowlStartGameGrab7Event
    {
        public long Bet;
        public long Current;
        public long Target;
        public long Reward;
    }

    class TreasureBowlStartGameAwardStateObject
    {
        public List<TreasureBowlStartGameBonus> Bonus;
        public long Award;
        public long Reward;
        public List<long> Line;
        public List<long> Reel1IconOrder;
        public List<long> Reel2IconOrder;
        public List<long> Reel3IconOrder;
    }

    class TreasureBowlStartGameBonus
    {
        public long Award;
        public long Reward;
        public List<long> Line;
    }
}
