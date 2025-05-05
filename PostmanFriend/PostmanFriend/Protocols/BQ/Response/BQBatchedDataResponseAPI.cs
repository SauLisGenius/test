using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.BQ.Response
{
    class BQBatchedDataResponseAPI
    {
        public List<BQBatchedDataDataResponse> dataResponse = new List<BQBatchedDataDataResponse>();
    }

    class BQBatchedDataDataResponse
    {
        public List<BQBatchedDataDataSubset> dataSubset = new List<BQBatchedDataDataSubset>();
    }

    class BQBatchedDataDataSubset
    {
        public BQBatchedDataDataset dataset;
    }

    class BQBatchedDataDataset
    {
        public BQBatchedDataTableDataSet tableDataset;
    }

    class BQBatchedDataTableDataSet
    {
        public List<BQBatchedDataColumn> column = new List<BQBatchedDataColumn>();
    }

    class BQBatchedDataColumn
    {
        public BQBatchedDataLongColumn longColumn;
        public BQBatchedDataDoubleColumn doubleColumn;
    }

    class BQBatchedDataLongColumn
    {
        public List<int> values = new List<int>();
    }

    class BQBatchedDataDoubleColumn
    {
        public List<int> values = new List<int>();
    }
}
