using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols
{
    class GetMachineListAPI
    {
        public GetMachineListServerData svData;
    }

    class GetMachineListServerData
    {
        public List<OtherMachineList> otherMachineList;
        public OwnMachineInfo ownMachineInfo;
    }

    class OtherMachineList
    {
        public long machineNo;
        public bool online;
        public bool allowEnterTransferCode;
    }

    class OwnMachineInfo {
        public long zoneType;
        public long machineNo;
    }
}
