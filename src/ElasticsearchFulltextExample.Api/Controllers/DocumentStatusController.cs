// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Database.Model;
using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Database.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Api.Controllers
{
    public class DocumentStatusController : Controller
    {
        private readonly ApplicationDbContextFactory applicationDbContextFactory;

        public DocumentStatusController(ApplicationDbContextFactory applicationDbContextFactory)
        {
            this.applicationDbContextFactory = applicationDbContextFactory;
        }

        [HttpGet]
        [Route("/api/status")]
        public async Task<IActionResult> Query(CancellationToken cancellationToken)
        {
            var documentStatusList = await GetDocumentStatusListAsync(cancellationToken);

            return Ok(documentStatusList);
        }

        private async Task<List<DocumentStatusDto>> GetDocumentStatusListAsync(CancellationToken cancellationToken)
        {
            using (var context = applicationDbContextFactory.Create())
            {
                using (var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted))
                {
                    return await context.Documents
                        // Project, so we do not load binary data:
                        .Select(document => new DocumentStatusDto
                        {
                            Id = document.Id,
                            Title = document.Title,
                            Filename = document.Filename,
                            IsOcrRequested = document.IsOcrRequested,
                            Status = ConvertStatusEnum(document.Status)
                        })
                        // Order By ID for now:
                        .OrderBy(x => x.Id)
                        // Do not track this query
                        .AsNoTracking()
                        // Evaluate Asynchronously:
                        .ToListAsync(cancellationToken);
                }
            }
        }


        [HttpDelete]
        [Route("/api/status/{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            using (var context = applicationDbContextFactory.Create())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    var document = await context.Documents.FirstAsync(x => x.Id == id, cancellationToken);

                    if (document == null)
                    {
                        return NotFound();
                    }

                    if (document.Status != ProcessingStatusEnum.Deleted)
                    {
                        document.Status = ProcessingStatusEnum.ScheduledDelete;

                        await context.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                    }
                }
            }

            return Ok();
        }

        [HttpPost]
        [Route("/api/status/{id}/index")]
        public async Task<IActionResult> Index(int id, CancellationToken cancellationToken)
        {
            using (var context = applicationDbContextFactory.Create())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    var document = await context.Documents.FirstAsync(x => x.Id == id, cancellationToken);

                    if (document == null)
                    {
                        return NotFound();
                    }

                    document.Status = ProcessingStatusEnum.ScheduledIndex;

                    await context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
            }

            return Ok();
        }


        private static StatusEnumDto ConvertStatusEnum(ProcessingStatusEnum status)
        {
            switch (status)
            {
                case ProcessingStatusEnum.None:
                    return StatusEnumDto.None;
                case ProcessingStatusEnum.ScheduledIndex:
                    return StatusEnumDto.ScheduledIndex;
                case ProcessingStatusEnum.ScheduledDelete:
                    return StatusEnumDto.ScheduledDelete;
                case ProcessingStatusEnum.Indexed:
                    return StatusEnumDto.Indexed;
                case ProcessingStatusEnum.Failed:
                    return StatusEnumDto.Failed;
                case ProcessingStatusEnum.Deleted:
                    return StatusEnumDto.Deleted;
                default:
                    return StatusEnumDto.None;
            }
        }
    }
}