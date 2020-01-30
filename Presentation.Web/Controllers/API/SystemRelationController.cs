using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Model.SystemUsage;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
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

            return result.Match
            (
                onSuccess: systemRelation =>
                    Created
                    (
                        response: MapRelation(systemRelation),
                        location: new Uri($"{Request.RequestUri.ToString().TrimEnd('/')}/from/{systemRelation.RelationSourceId}/{systemRelation.Id}")
                    ),
                onFailure: FromOperationError
            );
        }

        [HttpGet]
        [Route("from/{systemUsageId}")]
        public HttpResponseMessage GetRelationsFromSystem(int systemUsageId)
        {
            return _usageService.GetRelations(systemUsageId)
                .Match
                (
                    onSuccess: value => Ok(MapRelations(value)),
                    onFailure: FromOperationError
                );
        }

        [HttpGet]
        [Route("from/{systemUsageId}/{relationId}")]
        public HttpResponseMessage GetRelationFromSystem(int systemUsageId, int relationId)
        {
            return _usageService.GetRelation(systemUsageId, relationId)
                .Match
                (
                    onSuccess: relation => Ok(MapRelation(relation)),
                    onFailure: FromOperationFailure
                );
        }

        [HttpGet]
        [Route("options/{systemUsageId}/available-destination-systems")]
        public HttpResponseMessage GetAvailableDestinationSystems(int systemUsageId, string nameContent, int amount)
        {
            return _usageService.GetAvailableRelationTargets(systemUsageId, nameContent.FromString(), amount)
                .Match
                (
                    onSuccess: systemUsages => Ok(systemUsages.Select(x => x.MapToNamedEntityDTO()).ToList()),
                    onFailure: FromOperationError
                );
        }

        [HttpGet]
        [Route("options/{systemUsageId}/in-relation-to/{targetUsageId}")]
        public HttpResponseMessage GetAvailableOptions(int systemUsageId, int targetUsageId)
        {
            return _usageService.GetAvailableOptions(systemUsageId, targetUsageId)
                .Match
                (
                    onSuccess: options => Ok(MapOptions(options)),
                    onFailure: FromOperationError
                );
        }

        [HttpDelete]
        [Route("from/{systemUsageId}/{relationId}")]
        public HttpResponseMessage DeleteRelationsFromSystem(int systemUsageId, int relationId)
        {
            return _usageService.RemoveRelation(systemUsageId, relationId)
                .Match
                (
                    onSuccess: _ => NoContent(),
                    onFailure: FromOperationFailure
                );
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

        private static SystemRelationOptionsDTO MapOptions(RelationOptionsDTO options)
        {
            return new SystemRelationOptionsDTO
            {
                AvailableContracts = options.AvailableContracts.Select(contract => contract.MapToNamedEntityDTO()).ToArray(),
                AvailableFrequencyTypes = options.AvailableFrequencyTypes.Select(contract => contract.MapToNamedEntityDTO()).ToArray(),
                AvailableInterfaces = options.AvailableInterfaces.Select(contract => contract.MapToNamedEntityDTO()).ToArray(),
            };
        }
    }
}