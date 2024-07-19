// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Client.Infrastructure;
using ElasticsearchFulltextExample.Shared.Constants;
using Microsoft.Extensions.Localization;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ElasticsearchFulltextExample.Web.Client.Pages
{
    /// <summary>
    /// Code-Behind for the Upload.
    /// </summary>
    public partial class Upload
    {
        /// <summary>
        /// Holds the EditForm Data.
        /// </summary>
        public class UploadModel
        {
            /// <summary>
            /// Title of the Document.
            /// </summary>
            public string? Title { get; set; }

            /// <summary>
            /// Current Keyword.
            /// </summary>
            public string? Keyword { get; set; }

            /// <summary>
            /// Lists of Keywords, that have been added.
            /// </summary>
            public List<string> Keywords { get; set; } = new();

            /// <summary>
            /// Current Suggestion.
            /// </summary>
            public string? Suggestion { get; set; }

            /// <summary>
            /// List of Suggestions, that have been added.
            /// </summary>
            public List<string> Suggestions { get; set; } = new();

            /// <summary>
            /// Document Filename to be uploaded.
            /// </summary>
            public string? Filename { get; set; }

            /// <summary>
            /// File Stream, which is going to be uploaded.
            /// </summary>
            public Stream? File { get; set; }
        }

        /// <summary>
        /// We need to prevent the form from submitting, when Enter is 
        /// pressed for Keywords and Suggestions. We simply toggle this 
        /// value.
        /// </summary>
        bool canSubmitForm = false;

        /// <summary>
        /// The Reference to the File Uploader Element.
        /// </summary>
        FluentInputFile? myFileUploader = default!;

        /// <summary>
        /// The Current Upload Model representing the Form State.
        /// </summary>
        public UploadModel CurrentUpload = new UploadModel();

        /// <summary>
        /// When the Upload is done, this method is called.
        /// </summary>
        /// <param name="files">Files to be uploaded</param>
        /// <returns>An awaitable Task</returns>
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

        /// <summary>
        /// Handles the Keydown Event for the Keyword.
        /// </summary>
        public void OnKeywordEnter(KeyboardEventArgs e)
        {
            if(e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                canSubmitForm = false;

                OnAddKeyword();
            }
        }

        /// <summary>
        /// Adds a Keyword to the current list of Keywords.
        /// </summary>
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

        /// <summary>
        /// Removes a Keyword.
        /// </summary>
        /// <param name="keyword">Keyword to remove</param>
        public void RemoveKeyword(string keyword)
        {
            CurrentUpload.Keywords.Remove(keyword);
        }

        /// <summary>
        /// Handles the Keydown Event for the Suggestion.
        /// </summary>
        /// <param name="e"></param>
        public void OnSuggestionEnter(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                canSubmitForm = false;

                OnAddSuggestion();
            }
        }

        /// <summary>
        /// Adds a suggestion to the list of suggestions.
        /// </summary>
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


        /// <summary>
        /// Removes a suggestion.
        /// </summary>
        /// <param name="suggestion">Suggestion to remove</param>
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

            // The Multipart Content, we are going to upload.
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();

            // If there's a Title, add it as StringContent.
            if (CurrentUpload.Title != null)
            {
                multipartFormDataContent.Add(new StringContent(CurrentUpload.Title), FileUploadNames.Title);
            }

            // If there's a Filename and a File, we add it as StreamContent.
            if (CurrentUpload.Filename != null && CurrentUpload.File != null)
            {
                multipartFormDataContent.Add(new StreamContent(CurrentUpload.File), FileUploadNames.Data, CurrentUpload.Filename);
            }

            // Suggestions will be added as suggestion[0], suggestion[1], ... so the ASP.NET Core Binder turns them into a list
            if (CurrentUpload.Suggestions.Any())
            {
                for (var suggestionIdx = 0; suggestionIdx < CurrentUpload.Suggestions.Count; suggestionIdx++)
                {
                    multipartFormDataContent.Add(new StringContent(CurrentUpload.Suggestions[suggestionIdx]), $"{FileUploadNames.Suggestions}[{suggestionIdx}]");
                }
            }

            // Keywords will be added as keyword[0], keyword[1], ... so the ASP.NET Core Binder turns them into a list
            if (CurrentUpload.Keywords.Any())
            {
                for (var keywordIdx = 0; keywordIdx < CurrentUpload.Keywords.Count; keywordIdx++)
                {
                    multipartFormDataContent.Add(new StringContent(CurrentUpload.Keywords[keywordIdx]), $"{FileUploadNames.Keywords}[{keywordIdx}]");
                }
            }

            // Upload the MultipartFormData to the Server.
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