﻿using System;
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

    /// <summary>
    /// Relationer beskriver hvordan to lokale systemer er relateret til hinanden.
    /// </summary>
    [PublicApi]
    [RoutePrefix("api/v1/systemrelations")]
    public class SystemRelationController : BaseApiController
    {
        private readonly IItSystemUsageService _usageService;

        public SystemRelationController(IItSystemUsageService usageService)
        {
            _usageService = usageService;
        }

        /// <summary>
        /// Opretter en ny systemrelation
        /// </summary>
        /// <param name="relation"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public HttpResponseMessage PostRelation([FromBody] CreateSystemRelationDTO relation)
        {
            if (relation == null)
                return BadRequest("Missing relation data");

            var result = _usageService.AddRelation(
                relation.FromUsageId,
                relation.ToUsageId,
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
                        location: new Uri($"{Request.RequestUri.ToString().TrimEnd('/')}/from/{systemRelation.FromSystemUsageId}/{systemRelation.Id}")
                    ),
                onFailure: FromOperationError
            );
        }

        /// <summary>
        /// Henter alle systemrelationer FRA systemanvendelsen specificeret af <see cref="systemUsageId"/>
        /// </summary>
        /// <param name="systemUsageId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("from/{systemUsageId}")]
        public HttpResponseMessage GetRelationsFromSystem(int systemUsageId)
        {
            return _usageService.GetRelationsFrom(systemUsageId)
                .Match
                (
                    onSuccess: value => Ok(MapRelations(value)),
                    onFailure: FromOperationError
                );
        }

        /// <summary>
        /// Henter alle systemrelationer TIL systemanvendelsen specificeret af <see cref="systemUsageId"/>
        /// </summary>
        /// <param name="systemUsageId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("to/{systemUsageId}")]
        public HttpResponseMessage GetRelationsToSystem(int systemUsageId)
        {
            return _usageService.GetRelationsTo(systemUsageId)
                .Match
                (
                    onSuccess: value => Ok(MapRelations(value)),
                    onFailure: FromOperationError
                );
        }

        /// <summary>
        /// Henter alle systemrelationer der er relateret til kontrakten specificeret af <see cref="contractId"/>
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("associated-with/contract/{contractId}")]
        public HttpResponseMessage GetRelationsAssociatedWithContract(int contractId)
        {
            return _usageService.GetRelationsAssociatedWithContract(contractId)
                .Match
                (
                    onSuccess: value => Ok(MapRelations(value)),
                    onFailure: FromOperationError
                );
        }

        /// <summary>
        /// Henter en enkelt systemrelation
        /// </summary>
        /// <param name="systemUsageId">Systemanvendelsen der ejer relation</param>
        /// <param name="relationId">Id på systemrelationen</param>
        /// <returns></returns>
        [HttpGet]
        [Route("from/{systemUsageId}/{relationId}")]
        public HttpResponseMessage GetRelationFromSystem(int systemUsageId, int relationId)
        {
            return _usageService.GetRelationFrom(systemUsageId, relationId)
                .Match
                (
                    onSuccess: relation => Ok(MapRelation(relation)),
                    onFailure: FromOperationFailure
                );
        }

        /// <summary>
        /// Henter en liste over lokale it-systemer som systemet med id: <see cref="fromSystemUsageId"/> kan relateres til.
        /// </summary>
        /// <param name="fromSystemUsageId"></param>
        /// <param name="nameContent">valgfri navnesøgning</param>
        /// <param name="amount">Antal resultater der ønskers (maksimum 25)</param>
        /// <returns></returns>
        [HttpGet]
        [Route("options/{fromSystemUsageId}/systems-which-can-be-related-to")]
        public HttpResponseMessage GetSystemUsagesWhichCanBeRelatedTo(int fromSystemUsageId, string nameContent, int amount)
        {
            return _usageService.GetSystemUsagesWhichCanBeRelatedTo(fromSystemUsageId, nameContent.FromString(), amount)
                .Match
                (
                    onSuccess: systemUsages => Ok
                    (
                        systemUsages
                            .Select(x => x.MapToNamedEntityDTO())
                            .OrderBy(x => x.Name)
                            .ToList()
                    ),
                    onFailure: FromOperationError
                );
        }

        /// <summary>
        /// Henter en liste over gyldige valgmuligheder til oprettelse af relationen.
        /// </summary>
        /// <param name="fromSystemUsageId"></param>
        /// <param name="toSystemUsageId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("options/{fromSystemUsageId}/in-relation-to/{toSystemUsageId}")]
        public HttpResponseMessage GetAvailableOptions(int fromSystemUsageId, int toSystemUsageId)
        {
            return _usageService.GetAvailableOptions(fromSystemUsageId, toSystemUsageId)
                .Match
                (
                    onSuccess: options => Ok(MapOptions(options)),
                    onFailure: FromOperationError
                );
        }

        /// <summary>
        /// Sletter relationen
        /// </summary>
        /// <param name="fromSystemUsageId"></param>
        /// <param name="relationId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("from/{fromSystemUsageId}/{relationId}")]
        public HttpResponseMessage DeleteRelationFromSystem(int fromSystemUsageId, int relationId)
        {
            return _usageService.RemoveRelation(fromSystemUsageId, relationId)
                .Match
                (
                    onSuccess: _ => NoContent(),
                    onFailure: FromOperationFailure
                );
        }

        /// <summary>
        /// Opdaterer den eksisterende relation jf. værdierne i <see cref="relation"/>
        /// </summary>
        /// <param name="relation"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("")]
        public HttpResponseMessage PatchRelation([FromBody] SystemRelationDTO relation)
        {
            if (relation == null)
            {
                return BadRequest("Missing relation data");
            }

            if (relation.FromUsage == null || relation.ToUsage == null)
            {
                return BadRequest("FromUsage AND ToUsage MUST be defined");
            }

            var result = _usageService.ModifyRelation
            (
                relation.FromUsage.Id,
                relation.Id,
                relation.ToUsage.Id,
                relation.Description,
                relation.Reference,
                relation.Interface?.Id,
                relation.Contract?.Id,
                relation.FrequencyType?.Id
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
                Uuid = relation.Uuid,
                FromUsage = relation.FromSystemUsage.MapToNamedEntityDTO(),
                ToUsage = relation.ToSystemUsage.MapToNamedEntityDTO(),
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
                AvailableContracts =
                    options
                        .AvailableContracts
                        .Select(contract => contract.MapToNamedEntityDTO())
                        .OrderBy(x => x.Name)
                        .ToArray(),
                AvailableFrequencyTypes =
                    options
                        .AvailableFrequencyTypes
                        .Select(contract => contract.MapToNamedEntityDTO())
                        .OrderBy(x => x.Name)
                        .ToArray(),
                AvailableInterfaces =
                    options
                        .AvailableInterfaces
                        .Select(contract => contract.MapToNamedEntityDTO())
                        .OrderBy(x => x.Name)
                        .ToArray(),
            };
        }

        #endregion
    }
}