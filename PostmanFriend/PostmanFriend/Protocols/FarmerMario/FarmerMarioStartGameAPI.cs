using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.FarmerMario
{
    class FarmerMarioStartGameAPI
    {
        public string result;
    }

    class FarmerMarioStartGameResult {
        public long PlayerBalance;//身上鑽幣
        public List<List<long>> BoardResult;//停下的位置
        public long BoardReward;//獲得鑽幣(得分)
        public List<long> SlotResult;//中間輪盤圖示
        public long SlotReward;//中間輪盤(得分)

        public long SlotPrizePoolChicken;//公雞彩金
        public long SlotPrizePoolBar;//BAR3彩金
        public long SlotPrizePoolSeven;//紅7彩金
    }


}
