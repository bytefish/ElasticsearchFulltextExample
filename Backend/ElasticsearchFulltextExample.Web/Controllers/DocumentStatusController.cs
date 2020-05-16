// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Database.Factory;
using ElasticsearchFulltextExample.Web.Database.Model;
using ElasticsearchFulltextExample.Web.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nest;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Controllers
{
    public class DocumentStatusController : Controller
    {
        private readonly ApplicationOptions applicationOptions;
        private readonly ApplicationDbContextFactory applicationDbContextFactory;

        public DocumentStatusController(IOptions<ApplicationOptions> applicationOptions, ApplicationDbContextFactory applicationDbContextFactory)
        {
            this.applicationOptions = applicationOptions.Value;
            this.applicationDbContextFactory = applicationDbContextFactory;
        }

        [HttpGet]
        [Route("/api/status")]
        public async Task<IActionResult> Query(CancellationToken cancellationToken)
        {
            using(var context = applicationDbContextFactory.Create())
            {
                var documentStatus = await context.Documents
                    // Project, so we do not load binary data:
                    .Select(document => new DocumentStatusDto
                    {
                        DocumentId = document.DocumentId,
                        Title = document.Title,
                        Filename = document.Filename,
                        IsOcrRequested = document.IsOcrRequested,
                        Status = ConvertStatusEnum(document.Status)
                    })
                    // Do not track this query
                    .AsNoTracking()
                    // Evaluate Asynchronously:
                    .ToListAsync(cancellationToken);

                return Ok(documentStatus);
            }
        }

        private static StatusEnumDto ConvertStatusEnum(StatusEnum status)
        {
            switch(status)
            {
                case StatusEnum.None:
                    return StatusEnumDto.None;
                case StatusEnum.ScheduledIndex:
                    return StatusEnumDto.ScheduledIndex;
                case StatusEnum.ScheduledDelete:
                    return StatusEnumDto.ScheduledDelete;
                case StatusEnum.Indexed:
                    return StatusEnumDto.Indexed;
                case StatusEnum.Failed:
                    return StatusEnumDto.Failed;
                case StatusEnum.Deleted:
                    return StatusEnumDto.Deleted;
                default:
                    return StatusEnumDto.None;
            }
        }
    }
}