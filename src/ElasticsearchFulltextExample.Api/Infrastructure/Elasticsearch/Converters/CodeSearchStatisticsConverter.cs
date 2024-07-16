// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch.IndexManagement;
using ElasticsearchFulltextExample.Api.Models;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch.Converters
{
    public static class SearchStatisticsConverter
    {
        public static List<SearchStatistics> Convert(IndicesStatsResponse indicesStatsResponse)
        {
            if (indicesStatsResponse.Indices == null)
            {
                throw new Exception("No statistics available");
            }

            return indicesStatsResponse.Indices
                .Select(x => Convert(x.Key, x.Value))
                .ToList();
        }

        public static SearchStatistics Convert(string indexName, IndicesStats indexStats)
        {
            return new SearchStatistics
            {
                IndexName = indexName,
                IndexSizeInBytes = indexStats.Total?.Store?.SizeInBytes,
                TotalNumberOfDocumentsIndexed = indexStats.Total?.Docs?.Count,
                NumberOfDocumentsCurrentlyBeingIndexed = indexStats.Total?.Indexing?.IndexCurrent,
                TotalNumberOfFetches = indexStats.Total?.Search?.FetchTotal,
                NumberOfFetchesCurrentlyInProgress = indexStats.Total?.Search?.FetchCurrent,
                TotalNumberOfQueries = indexStats.Total?.Search?.QueryTotal,
                NumberOfQueriesCurrentlyInProgress = indexStats.Total?.Search?.QueryCurrent,
                TotalTimeSpentBulkIndexingDocumentsInMilliseconds = indexStats.Total?.Bulk?.TotalTimeInMillis,
                TotalTimeSpentIndexingDocumentsInMilliseconds = indexStats.Total?.Bulk?.TotalTimeInMillis,
                TotalTimeSpentOnFetchesInMilliseconds = indexStats.Total?.Search?.FetchTimeInMillis,
                TotalTimeSpentOnQueriesInMilliseconds = indexStats.Total?.Search?.QueryTimeInMillis,
            };
        }
    }
}
