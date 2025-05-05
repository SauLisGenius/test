using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.MiaoJi
{
    class MiaoJiStartGameAPI
    {
        public long type;
        public string invocationId;
        public string result;
    }
    
    class MiaoJiStartGameResult
    {
        public long PlayerBalance;
        public long NoBonusTimes;
        public MiaoJiStartGameGrab7Event Grab7Event;
        public long AwardState;
        public MiaoJiStartGameAwardStateObject AwardStateObject;
    }

    class MiaoJiStartGameResult2
    {
        public long PlayerBalance;
        public List<MiaoJiStartGameBetAndAward> BetAndAward;
        public long NoBonusTimes;
        public List<MiaoJiStartGameGrab7Event> Grab7Event;
        public long AwardState;
        public MiaoJiStartGameAwardStateObject AwardStateObject;
    }

    class MiaoJiStartGameBetAndAward
    {
        public long Bet;
        public long Award;
    }

    class MiaoJiStartGameGrab7Event 
    {
        public long Bet;
        public long Current;
        public long Target;
        public long Reward;
    }

    class MiaoJiStartGameAwardStateObject
    {
        public List<MiaoJiStartGameBonus> Bonus;
        public long Award;
        public long Reward;
        public List<long> Line;
        public List<long> Reel1IconOrder;
        public List<long> Reel2IconOrder;
        public List<long> Reel3IconOrder;
    }

    class MiaoJiStartGameBonus {
        public long Award;
        public long Reward;
        public List<long> Line;
    }
}
