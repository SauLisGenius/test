using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.Fruit
{
    class FruitLeaveMachineAPI
    {
        public FruitLeaveMachineServerData svData;
    }

    class FruitLeaveMachineServerData {
        public string machineKeepMessage;
        public string transferCode;
    }
}
