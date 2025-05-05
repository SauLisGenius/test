using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PostmanFriend.Protocols.PushChess
{
    class PushChessCL_GameStatusAPI
    {
        public string target;
        public List<object> arguments;
    }

    //-------------------------------------------------------

    class PushChessCL_GameStatusArguments
    {
        public PushChessCL_GameStatusObjData objData;
    }

    class PushChessCL_GameStatusObjData {
        public long eStatus;
        public long iTimer;
        public PushChessCL_GameStatusObjDataObjData objData;
    }

    class PushChessCL_GameStatusObjDataObjData {
        public PushChessCL_GameStatusObjDataObjDataBanker banker;
        public PushChessCL_GameStatusObjDataObjDataPdaRecord pdaRecord;
    }

    class PushChessCL_GameStatusObjDataObjDataBanker
    {
        public string sNickname;
    }

    class PushChessCL_GameStatusObjDataObjDataPdaRecord {
        public bool isChuWin;
        public bool isChuanWin;
        public bool isWeiWin;
    }

    //class PushChessCL_GameStatusNumber
    //{
    //    public bool isBankerWin;//莊
    //    public bool isChuWin;//出
    //    public bool isChuanWin;//川
    //    public bool isWeiWin;//尾
    //}

    //class PushChessCL_GameStatusObjData
    //{
    //    public PushChessCL_GameStatusDicRoad dicRoad;
    //}

    //class PushChessCL_GameStatusDicRoad {
    //    [JsonProperty(PropertyName = "0")]
    //    public PushChessCL_GameStatusNumber num0;

    //    [JsonProperty(PropertyName = "1")]
    //    public PushChessCL_GameStatusNumber num1;

    //    [JsonProperty(PropertyName = "2")]
    //    public PushChessCL_GameStatusNumber num2;

    //    [JsonProperty(PropertyName = "3")]
    //    public PushChessCL_GameStatusNumber num3;

    //    [JsonProperty(PropertyName = "4")]
    //    public PushChessCL_GameStatusNumber num4;
    //}


    //---------------------------------------------------------

    class PushChessBets {
        public long Chu;
        public long Chuan;
        public long Wei;
    }


}
