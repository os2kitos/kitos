using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
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

            return result.Match(
                onSuccess: systemRelation =>
                    Created
                    (
                        response: MapRelation(systemRelation),
                        location: new Uri($"{Request.RequestUri.ToString().TrimEnd('/')}/from/{systemRelation.RelationSourceId}/{systemRelation.Id}")
                    ),
                onFailure: FromOperationError);
        }

        [HttpGet]
        [Route("from/{systemUsageId}")]
        public HttpResponseMessage GetRelationsFromSystem(int systemUsageId)
        {
            var result = _usageService.GetRelations(systemUsageId);

            return result.Match(
                onSuccess: value => Ok(MapRelations(value)),
                onFailure: FromOperationError);
        }

        [HttpGet]
        [Route("from/{systemUsageId}/{relationId}")]
        public HttpResponseMessage GetRelationsFromSystem(int systemUsageId, int relationId)
        {
            var result = _usageService.GetRelation(systemUsageId, relationId);

            return result.Match(
                onSuccess: relation => Ok(MapRelation(relation)),
                onFailure: FromOperationFailure);
        }

        [HttpDelete]
        [Route("from/{systemUsageId}/{relationId}")]
        public HttpResponseMessage DeleteRelationsFromSystem(int systemUsageId, int relationId)
        {
            var result = _usageService.RemoveRelation(systemUsageId, relationId);

            return result.Match(
                onSuccess: _ => NoContent(),
                onFailure: FromOperationFailure);
        }

        [HttpPatch]
        [Route("")]
        public HttpResponseMessage PatchRelation([FromBody] SystemRelationDTO relation)
        {
            if (relation == null)
            {
                return BadRequest("Missing relation data");
            }

            if (relation.Source == null || relation.Destination == null)
            {
                return NotFound();
            }

            var result = _usageService.ModifyRelation(
                relation.Source.Id,
                relation.Id,
                relation.Destination.Id,
                relation.Interface?.Id
                );

            return result.Match(onSuccess: systemRelation => Ok(MapRelation(systemRelation)), onFailure: FromOperationError);
        }

        #region Helpers

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

        #endregion
    }
}