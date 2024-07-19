// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Client.Infrastructure;
using ElasticsearchFulltextExample.Shared.Constants;
using Microsoft.Extensions.Localization;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

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

            public Stream? File { get; set; }
        }

        bool canSubmitForm = false;

        FluentInputFile? myFileUploader = default!;

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
            CurrentUpload.File = file.Stream;

            return Task.CompletedTask;
        }

        public void OnKeywordEnter(KeyboardEventArgs e)
        {
            if(e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                canSubmitForm = false;

                OnAddKeyword();
            }
        }

        public void OnAddKeyword()
        {
            var keyword = CurrentUpload.Keyword;

            if(keyword == null)
            {
                return;
            }

            if (CurrentUpload.Keywords.Contains(keyword))
            {
                return;
            }
            
            CurrentUpload.Keywords.Add(keyword);
        }

        public void RemoveKeyword(string keyword)
        {
            CurrentUpload.Keywords.Remove(keyword);
        }

        public void OnSuggestionEnter(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                canSubmitForm = false;

                OnAddSuggestion();
            }
        }

        public void OnAddSuggestion()
        {
            var suggestion = CurrentUpload.Suggestion;

            if (suggestion == null)
            {
                return;
            }

            if (CurrentUpload.Suggestions.Contains(suggestion))
            {
                return;
            }

            CurrentUpload.Suggestions.Add(suggestion);
        }


        public void RemoveSuggestion(string suggestion)
        {
            CurrentUpload.Suggestions.Remove(suggestion);
        }

        /// <summary>
        /// Submits the Form and reloads the updated data.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleValidSubmitAsync()
        {
            // We came from an Enter, do not submit yet ...
            if(!canSubmitForm)
            {
                canSubmitForm = true;

                return;
            }

            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();

            if (CurrentUpload.Title != null)
            {
                multipartFormDataContent.Add(new StringContent(CurrentUpload.Title), FileUploadNames.Title);
            }

            if (CurrentUpload.Filename != null && CurrentUpload.File != null)
            {
                multipartFormDataContent.Add(new StreamContent(CurrentUpload.File), FileUploadNames.Data, CurrentUpload.Filename);
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
            if (string.IsNullOrWhiteSpace(upload.Title))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(upload.Title),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(upload.Title))
                };
            }
        }
    }
}