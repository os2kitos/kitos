using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Presentation.Web.Models.SystemRelations;

namespace Presentation.Web.Controllers.API
{

    [InternalApi]
    [RoutePrefix("api/v1/systemrelations")]
    public class SystemRelationController : BaseApiController
    {
        private readonly IItSystemUsageService _usageService;

        public SystemRelationController(IItSystemUsageService usageService)
        {
            _usageService = usageService;
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage PostRelation([FromBody] CreateSystemRelationDTO relation)
        {
            if (relation == null)
                return BadRequest("Missing relation data");

            var result = _usageService.AddRelation(
                relation.SourceUsageId,
                relation.TargetUsageId,
                relation.InterfaceId,
                relation.Description,
                relation.Reference,
                relation.FrequencyTypeId,
                relation.ContractId);

            return Either(result,
                onSuccess: value => Created($"system-usages/{relation.SourceUsageId}/usage-relations/{value.Id}"),
                onFailure: FromOperationError);
        }

        [HttpGet]
        [Route("from/{systemUsageId}")]
        public HttpResponseMessage GetRelationsFromSystem(int systemUsageId)
        {
            var result = _usageService.GetRelations(systemUsageId);

            return Either(result,
                onSuccess: value => Ok(MapRelations(value)),
                onFailure: FromOperationError);
        }

        private static SystemRelationDTO[] MapRelations(IEnumerable<SystemRelation> systemRelations)
        {
            return systemRelations
                .OrderBy(relation => relation.Id)
                .Select(MapRelation)
                .ToArray();
        }

        private static SystemRelationDTO MapRelation(SystemRelation relation)
        {
            return new SystemRelationDTO
            {
                Id = relation.Id,
                Source = relation.RelationSource.MapToNamedEntityDTO(),
                Destination = relation.RelationTarget.MapToNamedEntityDTO(),
                Description = relation.Description,
                Reference = relation.Reference,
                Contract = relation.AssociatedContract?.MapToNamedEntityDTO(),
                FrequencyType = relation.UsageFrequency?.MapToNamedEntityDTO(),
                Interface = relation.RelationInterface?.MapToNamedEntityDTO()
            };
        }
    }
}