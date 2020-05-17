// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Database.Factory;
using ElasticsearchFulltextExample.Web.Database.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Controllers
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
                using (var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted))
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

                    if (document.Status != StatusEnum.Deleted)
                    {
                        document.Status = StatusEnum.ScheduledDelete;

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

                    document.Status = StatusEnum.ScheduledIndex;

                    await context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
            }

            return Ok();
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