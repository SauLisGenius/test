using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.Ragnarok
{
    class RagnarokStartGameAPI
    {
        public RagnarokStartGameServerData svData;
    }

    class RagnarokStartGameServerData {
        public RagnarokStartGameStr str;
    }

    class RagnarokStartGameStr {
        public List<RagnarokStartGameResult> result;
        public long totalScore;
    }

    class RagnarokStartGameResult {
        public RagnarokStartGameSlotData slotData;
    }

    class RagnarokStartGameSlotData { 
        //"1" 我又不會寫了XD
    }
}
