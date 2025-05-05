using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.Fruit
{
    class FruitStartGameAPI
    {
        public FruitStartGameServerData svData;
    }

    class FruitStartGameServerData
    {
        public long iTotalWin;
        public FruitStartGameDcGameResult dcGameResult;
    }

    class FruitStartGameDcGameResult
    {
        public List<FruitStartGameNormal> Normal;
        public List<FruitStartGameBonus> Bonus;
        public List<FruitStartGameFreeGame> FreeGame;
    }

    class FruitStartGameNormal
    {
        public string sGameNo;
        public FruitStartGameSlotData slotData;
    }

    class FruitStartGameBonus
    {

    }

    class FruitStartGameFreeGame
    {

    }

    class FruitStartGameSlotData
    {
        public object aa;//"1" 我不會寫啦
    }
}
