using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.MuseStar
{
    class MuseStarStartGameAPI
    {
        public MuseStarStartGameServerData svData;
    }

    class MuseStarStartGameServerData {
        public MuseStarStartGameNormal normal;
        public List<MuseStarStartGameFreeGameResult> freeGame;
        public MuseStarStartGameBonusGame bonusGame;
    }

    class MuseStarStartGameNormal
    {
        public long win;
    }

    class MuseStarStartGameFreeGameResult
    {
        public long win;
    }

    class MuseStarStartGameBonusGame
    {
        public List<MuseStarStartGameBonusGameResult> firstStage;
        public List<MuseStarStartGameBonusGameResult> secondStage;
        public List<MuseStarStartGameBonusGameResult> thirdStage;
    }

    class MuseStarStartGameBonusGameResult
    {
        public long win;
    }


}
