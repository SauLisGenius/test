using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.Pisces2
{
    class Pisces2StartGameAPI
    {
        public int type;
        public string target;
        public object result;
        public List<object> arguments;
    }

    #region AddCredits
    class Pisces2StartGamePlayerInfo {
        public int playerUid;
        public int currencyType;
        public int creditCount;
        public int type;
        public int startBalance;
        public int endBalance;
    }
    #endregion

    #region Bet -> RefreshCurrentBet  Bet -> RefreshCurrentCredits
    class Pisces2Bet_RefreshCurrentBet
    {
        public int alreadyBet;//已經下注的注數
    }

    class Pisces2Bet_RefreshCurrentCredits
    {
        public int remainderCredits;//玩家身上剩餘Credits
    }
    #endregion

    #region BetAll -> RefreshCurrentBet  BetAll -> RefreshCurrentCredits
    class Pisces2BetAll_RefreshCurrentBet
    {
        public int alreadyBet;//已經下注的注數
    }

    class Pisces2BetAll_RefreshCurrentCredits
    {
        public int remainderCredits;//玩家身上剩餘Credits
    }
    #endregion

    #region StartGame
    class Pisces2StartGameBetResponse {
        public int phase;
        public List<Pisces2StartGamePokerInfo> pokerInfos;
        public List<Pisces2StartGameHintInfo> hintInfos;
    }

    class Pisces2StartGamePokerInfo {
        public int id;
        public int position;
    }

    class Pisces2StartGameHintInfo {
        public int position;
        public List<string> bringPokers;
    }
    #endregion

    #region DoubleUp -> RefreshDoubleUpResult
    class Pisces2StartGameRefreshDoubleUpResult
    {
        public int pokerId;
        public int totalWin;
        public bool isLockMachine;
    }
    #endregion
    
    #region AwardResult
    class Pisces2StartGameAwardResult
    {
        public int award;
        public long awardWin;
        public bool isDoubleWin;
        public bool isLockMachine;
    }
    #endregion

    #region GameResult -> RefreshEventInfo
    class Pisces2StartGameRefreshEventInfo
    {
        public int sevenFlushMaxBet;
        public int sevenFlushNotMaxBet;
        public int bigFullHouseMaxBet;
        public int bigFullHouseNotMaxBet;
    }
    #endregion

    class Pisces2StartGameRefreshExchangeInfo
    {
        public long currencyCount;
        public long creditCount;
        public long startBalance;
        public long endBalance;
    }
}
