// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Shared.Client;

namespace ElasticsearchFulltextExample.Web.Client.Pages
{
    public partial class Manage
    {
        /// <summary>
        /// Recreates the Search Index.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleRecreateSearchIndexAsync()
        {
            await SearchClient.DeleteSearchIndexAsync(default);
            await SearchClient.CreateSearchIndexAsync(default);
        }

        /// <summary>
        /// Creates the Search Index.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleCreateSearchIndexAsync()
        {
            await SearchClient.CreateSearchIndexAsync(default);
        }

        /// <summary>
        /// Deletes the Search Index.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleDeleteSearchIndexAsync()
        {
            await SearchClient.DeleteSearchIndexAsync(default);
        }

        /// <summary>
        /// Deletes all Documents from Index.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleDeleteAllDocumentsAsync()
        {
            await SearchClient.DeleteAllDocumentsAsync(default);
        }
    }
}