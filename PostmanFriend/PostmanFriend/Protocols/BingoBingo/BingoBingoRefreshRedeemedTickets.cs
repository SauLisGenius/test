using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.BingoBingo
{
    class BingoBingoRefreshRedeemedTickets
    {
        public List<string> arguments;
    }

    class BingoBingoRefreshRedeemedTicketsArguments {
        public float CurrencyAmount;//下注金額
        public long TotalBet;//總注數
        public float TotalWin;
    }

}
