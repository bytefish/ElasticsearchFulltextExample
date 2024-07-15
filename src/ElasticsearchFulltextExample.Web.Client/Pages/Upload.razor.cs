// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Web.Client.Infrastructure;
using ElasticsearchFulltextExample.Shared.Constants;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.FluentUI.AspNetCore.Components;

namespace ElasticsearchCodeSearch.Web.Client.Pages
{
    public partial class Upload
    {
        public class UploadModel
        {
            public string? Title { get; set; }

            public string? Name { get; set; }

            public string? Filename { get; set; }

            public long? Size { get; set; }

            public Stream? File { get; set; }
        }

        public UploadModel CurrentUpload = new UploadModel();

        private Task OnCompletedAsync(IEnumerable<FluentInputFileEventArgs> files)
        {
            if(files == null)
            {
                return Task.CompletedTask;
            }

            var file = files.FirstOrDefault();

            if(file == null)
            {
                return Task.CompletedTask;
            }

            if(file.Stream == null)
            {
                return Task.CompletedTask;
            }

            CurrentUpload.Filename = file.Name;
            CurrentUpload.Size = file.Size;
            CurrentUpload.File = new StreamContent(file.Stream);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Submits the Form and reloads the updated data.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleValidSubmitAsync()
        {
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();

            multipartFormDataContent.Add(new StringContent(CurrentUpload.Title), FileUploadNames.Title);
            multipartFormDataContent.Add(new StreamContent(CurrentUpload.File!, "", CurrentUpload.Filename!);

            await ElasticsearchCodeSearchService.IndexGitRepositoryAsync(CurrentGitRepository, default);

            CurrentGitRepository = new GitRepositoryMetadataDto
            {
                Branch = string.Empty,
                Name = string.Empty,
                CloneUrl = string.Empty,
                Owner = string.Empty,
                Source = SourceSystems.GitHub,
            };
        }

        /// <summary>
        /// Submits the Form and reloads the updated data.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private Task HandleDiscardAsync()
        {
            CurrentGitRepository = new GitRepositoryMetadataDto
            {
                Branch = string.Empty,
                Name = string.Empty,
                CloneUrl = string.Empty,
                Owner = string.Empty,
                Source = SourceSystems.GitHub
            };

            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates a <see cref="GitRepositoryMetadataDto"/>.
        /// </summary>
        /// <param name="repository">Item to validate</param>
        /// <returns>The list of validation errors for the EditContext model fields</returns>
        private IEnumerable<ValidationError> ValidateGitRepository(GitRepositoryMetadataDto repository)
        {
            if (string.IsNullOrWhiteSpace(repository.Owner))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(repository.Owner),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.Owner))
                };
            }

            if (string.IsNullOrWhiteSpace(repository.Name))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(repository.Name),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.Name))
                };
            }

            if (string.IsNullOrWhiteSpace(repository.Branch))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(repository.Branch),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.Branch))
                };
            }

            if (string.IsNullOrWhiteSpace(repository.CloneUrl))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(repository.CloneUrl),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.CloneUrl))
                };
            }

            if (string.IsNullOrWhiteSpace(repository.Source))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(repository.Source),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(repository.Source))
                };
            }
        }
    }
}