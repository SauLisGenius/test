using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostmanFriend.Protocols.BQ.Request
{
    class BQBatchedDataRequestAPI {
        public List<BQBatchedDataDataRequest> dataRequest;
    }

    class BQBatchedDataDataRequest
    {
        public BQBatchedDataRequestContext requestContext;
        public BQBatchedDataDatasetSpec datasetSpec;
        public BQBatchedDataRetryHints retryHints;
    }

    #region RequestContext
    class BQBatchedDataRequestContext {
        public BQBatchedDataReportContext reportContext;
    }

    class BQBatchedDataReportContext {
        public string reportId;
        public string pageId;
        public int mode;
        public string componentId;
        public string displayType;
        //public string actionId;
    }
    #endregion

    #region DatasetSpec
    class BQBatchedDataDatasetSpec {
        public List<BQBatchedDataDataset> dataset;
        public List<BQBatchedDataQueryFields> queryFields;
        public List<BQBatchedDataSortData> sortData;
        public bool includeRowsCount;
        public BQBatchedDataRelatedDimensionMask relatedDimensionMask;
        public BQBatchedDataPaginateInfo paginateInfo;
        ////public BQBatchedDataBlendConfig blendConfig;
        public List<BQBatchedDataFilters> filters;
        public List<BQBatchedDataFeatures> features;
        public List<BQBatchedDataDateRanges> dateRanges;
        public int contextNsCount;
        public List<BQBatchedDataCalculatedField> calculatedField;
        public bool needGeocoding;
        public List<BQBatchedDataGeoFieldMask> geoFieldMask;
        public int geoVertices;
    }

    #region Dataset
    class BQBatchedDataDataset {
        public string datasourceId;
        public int revisionNumber;
        public List<BQBatchedDataParameterOverrides> parameterOverrides;
    }

    class BQBatchedDataParameterOverrides { 
    
    }
    #endregion

    #region QueryFields
    class BQBatchedDataQueryFields {
        public string name;
        public string datasetNs;
        public string tableNs;
        public BQBatchedDataDataTransformation dataTransformation;
    }

    class BQBatchedDataDataTransformation {
        public string sourceFieldName;
        ////public string textFormula;
        ////public int sourceType;
        ////public string frontendTextFormula;
        ////public int formulaOutputDataType;
        public int aggregation;//kkk
    }
    #endregion

    #region SortData
    class BQBatchedDataSortData
    {
        public BQBatchedDataSortColumn sortColumn;
        public int sortDir;
    }

    class BQBatchedDataSortColumn {
        public string name;
        public string datasetNs;
        public string tableNs;
        public BQBatchedDataDataTransformation dataTransformation;
    }
    #endregion

    #region RelateDimensionMask
    class BQBatchedDataRelatedDimensionMask
    {
        public bool addDisplay;
        public bool addUniqueId;
        public bool addLatLong;
    }
    #endregion

    #region PaginateInfo
    class BQBatchedDataPaginateInfo
    {
        public int startRow;
        public int rowsCount;
    }
    #endregion

    #region BlendConfig
    class BQBatchedDataBlendConfig
    {
        public BQBatchedDataBlockDatasource blockDatasource;
    }

    class BQBatchedDataBlockDatasource
    {
        public List<BQBatchedDataBlocks> blocks;
        public BQBatchedDataDatasourceBlock datasourceBlock;
        public bool delegatedAccessEnabled;
        public bool isUnlocked;
        public bool isCacheable;
    }

    #region Blocks
    class BQBatchedDataBlocks
    {
        public string id;
        public int type;
        public List<BQBatchedDataInputBlockIds> inputBlockIds;
        public List<BQBatchedDataOutputBlockIds> outputBlockIds;
        public List<BQBatchedDataOutputBlockIds> fields;
        public bool isExperimental;
        public BQBatchedDataTreeQueryBlockConfig treeQueryBlockConfig;
    }

    class BQBatchedDataInputBlockIds { }

    class BQBatchedDataOutputBlockIds { }

    class BQBatchedDataFields
    {
        //public BQBatchedDataOutputSemanticInfo outputSemanticInfo;//
        public int columnType;
        public BQBatchedDataField field;
        public string outputName;
        public bool enabled;
        public int conceptType;
        public List<BQBatchedDataParams> @params;
        public int dataType;
        public List<BQBatchedDataProperty> property;
        public bool isRepeated;
        public bool isDefault;
        public List<BQBatchedDataAncestors> ancestors;
        public BQBatchedDataQueryTimeTransformation queryTimeTransformation;
    }



    class BQBatchedDataTreeQueryBlockConfig
    {
        public BQBatchedDataJoin join;
    }

    class BQBatchedDataJoin
    {
        public BQBatchedDataRight right;
        public BQBatchedDataLeftJoin left;
        public BQBatchedDataCondition condition;
        public int type;
    }

    class BQBatchedDataRight
    {
        public BQBatchedDataQuery query;
    }

    class BQBatchedDataQuery
    {
        public List<BQBatchedDataConcepts> concepts;
        public string datasourceId;
    }

    class BQBatchedDataConcepts
    {
        public BQBatchedDataId id;
        public List<int> semantic;
        public BQBatchedDataQueryTimeTransformation queryTimeTransformation;
        public bool isDummy;
    }

    class BQBatchedDataId
    {
        public string id;
        public string name;
        public string @namespace;//為了使用保留字namespace，所以在變數前面加上@
    }

    class BQBatchedDataLeftJoin
    {
        public BQBatchedDataJoin join;
        public BQBatchedDataQuery query;
    }

    class BQBatchedDataLeftQuery
    {
        public BQBatchedDataQuery query;
    }

    class BQBatchedDataCondition
    {
        public BQBatchedDataAnd and;
    }

    class BQBatchedDataAnd
    {
        public List<BQBatchedDataConditions> conditions;
    }

    class BQBatchedDataConditions
    {
        public BQBatchedDataBoolean boolean;
    }

    class BQBatchedDataBoolean
    {
        public BQBatchedDataJoinKeyPair joinKeyPair;
    }

    class BQBatchedDataJoinKeyPair
    {
        public string leftName;
        public string rightName;
    }
    #endregion

    #region DatasourceBlock
    class BQBatchedDataDatasourceBlock
    {
        public string id;
        public int type;
        public List<BQBatchedDataInputBlockIds> inputBlockIds;
        public List<BQBatchedDataOutputBlockIds> outputBlockIds;
        public List<BQBatchedDataFields> fields;
        public string name;
    }

    class BQBatchedDataOutputSemanticInfo
    {
        public List<BQBatchedDataSemanticConfig> semanticConfig;
    }

    class BQBatchedDataSemanticConfig
    {
        public int semanticType;
    }

    class BQBatchedDataField
    {
        public string ns;
        public string name;
        public string simpleName;
    }

    class BQBatchedDataParams
    {

    }

    class BQBatchedDataProperty
    {
    
    }

    class BQBatchedDataAncestors
    {
    
    }
    #endregion

    #endregion

    #region Filters
    class BQBatchedDataFilters {
        public BQBatchedDataFilterDefinition filterDefinition;
        public BQBatchedDataDataSubsetNs dataSubsetNs;
        public int version;
    }

    class BQBatchedDataFilterDefinition {
        public BQBatchedDataFilterExpression filterExpression;
    }

    class BQBatchedDataFilterExpression {
        public bool include;
        public int conceptType;
        public BQBatchedDataConcept concept;
        public string filterConditionType;
        public BQBatchedDataQueryTimeTransformation queryTimeTransformation;
        public List<string> stringValues;
    }

    class BQBatchedDataConcept {
        public string name;
        public string ns;
    }

    class BQBatchedDataQueryTimeTransformation {
        public BQBatchedDataDataTransformation dataTransformation;
        public BQBatchedDataDisplayTransformation displayTransformation;
        ////public bool isDummy;
    }

    class BQBatchedDataDisplayTransformation {
        public string displayName;
        ////public BQBatchedDataOutputSemanticInfo outputSemanticInfo;
    }

    class BQBatchedDataDataSubsetNs {
        public string datasetNs;
        public string tableNs;
        public string contextNs;
    }
    #endregion

    #region Features
    class BQBatchedDataFeatures
    {

    }
    #endregion

    #region DataRanges
    class BQBatchedDataDateRanges
    {
        public string startDate;
        public string endDate;
        public BQBatchedDataDataSubsetNs dataSubsetNs;
    }
    #endregion

    #region CalculatedField
    class BQBatchedDataCalculatedField
    {

    }
    #endregion

    #region GeoFieldMask
    class BQBatchedDataGeoFieldMask
    {

    }
    #endregion

    #endregion

    #region retryHints
    class BQBatchedDataRetryHints {
        public bool useClientControlledRetry;
        public bool isLastRetry;
        public int retryCount;
        public string originalRequestId;
    }
    #endregion
}
