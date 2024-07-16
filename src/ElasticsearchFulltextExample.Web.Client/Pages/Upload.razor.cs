// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Client.Infrastructure;
using ElasticsearchFulltextExample.Shared.Constants;
using Microsoft.Extensions.Localization;
using Microsoft.FluentUI.AspNetCore.Components;

namespace ElasticsearchFulltextExample.Web.Client.Pages
{
    public partial class Upload
    {
        public class UploadModel
        {
            public string? Title { get; set; }

            public string? Name { get; set; }

            public string? Keyword { get; set; }

            public List<string> Keywords { get; set; } = new();

            public string? Suggestion { get; set; }

            public List<string> Suggestions { get; set; } = new();

            public string? Filename { get; set; }

            public Stream? Data { get; set; }
        }

        public UploadModel CurrentUpload = new UploadModel();

        private Task OnCompletedAsync(IEnumerable<FluentInputFileEventArgs> files)
        {
            if (files == null)
            {
                return Task.CompletedTask;
            }

            var file = files.FirstOrDefault();

            if (file == null)
            {
                return Task.CompletedTask;
            }

            if (file.Stream == null)
            {
                return Task.CompletedTask;
            }

            CurrentUpload.Filename = file.Name;
            CurrentUpload.Data = file.Stream;

            return Task.CompletedTask;
        }

        private void OnAddSuggestion()
        {
            var suggestion = CurrentUpload.Suggestion;

            if (suggestion != null)
            {
                CurrentUpload.Suggestions.Add(suggestion);
            }
        }

        private void OnAddKeyword()
        {
            var keyword = CurrentUpload.Keyword;

            if (keyword != null)
            {
                CurrentUpload.Keywords.Add(keyword);
            }
        }

        /// <summary>
        /// Submits the Form and reloads the updated data.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleValidSubmitAsync()
        {
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();

            if (CurrentUpload.Title != null)
            {
                multipartFormDataContent.Add(new StringContent(CurrentUpload.Title), FileUploadNames.Title);
            }

            if (CurrentUpload.Filename != null && CurrentUpload.Data != null)
            {
                multipartFormDataContent.Add(new StreamContent(CurrentUpload.Data), FileUploadNames.Data, CurrentUpload.Filename);
            }

            if (CurrentUpload.Suggestions.Any())
            {
                for (var suggestionIdx = 0; suggestionIdx < CurrentUpload.Suggestions.Count; suggestionIdx++)
                {
                    multipartFormDataContent.Add(new StringContent(CurrentUpload.Suggestions[suggestionIdx]), $"{FileUploadNames.Suggestions}[{suggestionIdx}]");
                }
            }

            if (CurrentUpload.Keywords.Any())
            {
                for (var keywordIdx = 0; keywordIdx < CurrentUpload.Keywords.Count; keywordIdx++)
                {
                    multipartFormDataContent.Add(new StringContent(CurrentUpload.Keywords[keywordIdx]), $"{FileUploadNames.Keywords}.[{keywordIdx}]");
                }
            }

            await SearchClient
                .UploadAsync(multipartFormDataContent, default)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Submits the Form and reloads the updated data.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private Task HandleDiscardAsync()
        {
            CurrentUpload = new UploadModel();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates a <see cref="GitRepositoryMetadataDto"/>.
        /// </summary>
        /// <param name="repository">Item to validate</param>
        /// <returns>The list of validation errors for the EditContext model fields</returns>
        private IEnumerable<ValidationError> ValidateCurrentUpload(UploadModel upload)
        {
            if (string.IsNullOrWhiteSpace(upload.Name))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(upload.Name),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(upload.Name))
                };
            }
        }
    }
}