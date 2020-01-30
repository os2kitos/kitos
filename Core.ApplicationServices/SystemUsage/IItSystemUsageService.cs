using System.Collections.Generic;
using Core.ApplicationServices.Model.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.SystemUsage
{
    public interface IItSystemUsageService
    {
        Result<ItSystemUsage, OperationFailure> Add(ItSystemUsage usage, User objectOwner);
        Result<ItSystemUsage, OperationFailure> Delete(int id);
        ItSystemUsage GetByOrganizationAndSystemId(int organizationId, int systemId);
        ItSystemUsage GetById(int usageId);

        /// <summary>
        /// Adds a relation between two IT-systems
        /// </summary>
        /// <param name="sourceId">Source IT-system usage ID</param>
        /// <param name="destinationId">Destination IT-system usage ID</param>
        /// <param name="interfaceId">Optional interface id</param>
        /// <param name="description">Optional description</param>
        /// <param name="reference">Optional reference</param>
        /// <param name="frequencyId">Optional frequency id</param>
        /// <param name="contractId">Optional contract Id</param>
        /// <returns></returns>
        Result<SystemRelation, OperationError> AddRelation(int sourceId, int destinationId, int? interfaceId, string description, string reference, int? frequencyId, int? contractId);
        /// <summary>
        /// Gets all relations FROM the target system usage
        /// </summary>
        /// <param name="systemUsageId"></param>
        /// <returns></returns>
        Result<IEnumerable<SystemRelation>, OperationError> GetRelations(int systemUsageId);
        /// <summary>
        /// Removes a system relation from the specified system usage
        /// </summary>
        /// <param name="systemUsageId">Id of the "source" it system usage</param>
        /// <param name="relationId">Relation Id</param>
        /// <returns></returns>
        Result<SystemRelation, OperationFailure> RemoveRelation(int systemUsageId, int relationId);
        /// <summary>
        /// Gets a system relation
        /// </summary>
        /// <param name="systemUsageId"></param>
        /// <param name="relationId"></param>
        /// <returns></returns>
        Result<SystemRelation, OperationFailure> GetRelation(int systemUsageId, int relationId);

        /// <summary>
        /// Gets the systems which the target system can relate to
        /// </summary>
        /// <param name="systemUsageId"></param>
        /// <param name="nameContent"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Result<IEnumerable<ItSystemUsage>, OperationError> GetAvailableRelationTargets(int systemUsageId, Maybe<string> nameContent, int pageSize);
        /// <summary>
        /// Gets the valid options for the given system relation
        /// </summary>
        /// <param name="systemUsageId"></param>
        /// <param name="targetUsageId"></param>
        /// <returns></returns>
        Result<RelationOptionsDTO, OperationError> GetAvailableOptions(int systemUsageId, int targetUsageId);
    }
}