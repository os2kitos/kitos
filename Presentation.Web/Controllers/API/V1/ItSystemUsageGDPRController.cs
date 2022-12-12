using Presentation.Web.Infrastructure.Attributes;
using System;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    [RoutePrefix("api/v1/it-system-usage/{id}/gdpr")]
    public class ItSystemUsageGDPRController : BaseApiController
    {
        private readonly IItSystemUsageService _itSystemUsageService;

        public ItSystemUsageGDPRController(IItSystemUsageService itSystemUsageService)
        {
            _itSystemUsageService = itSystemUsageService;
        }

        [HttpPatch]
        [Route("planned-risk-assessment-date")]
        public HttpResponseMessage UpdatePlannedRiskAssessmentDate(int id, [FromBody] DateTime? date)
        {
            return _itSystemUsageService
                .UpdatePlannedRiskAssessmentDate(id, date)
                .Select(ToDTO)
                .Match(Ok, FromOperationError);
        }

        private ItSystemUsageDTO ToDTO(ItSystemUsage usage)
        {
            return Map<ItSystemUsage, ItSystemUsageDTO>(usage);
        }
    }
}