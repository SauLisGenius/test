using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.Baccarat
{
    class BaccaratCL_RoadAPI
    {
        public string target;
        public List<BaccaratCL_RoadArguments> arguments;
    }

    class BaccaratCL_RoadArguments
    {
        public long number;
        public long points;
        public long remain;
        public List<long> awards;
    }
}
