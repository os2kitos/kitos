using System.Collections.Generic;
using Core.ApplicationServices.Model.SystemUsage;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.SystemUsage
{
    public interface IItSystemUsageService
    {
        Result<ItSystemUsage, OperationFailure> Add(ItSystemUsage usage);
        Result<ItSystemUsage, OperationFailure> Delete(int id);
        ItSystemUsage GetByOrganizationAndSystemId(int organizationId, int systemId);
        ItSystemUsage GetById(int usageId);

        /// <summary>
        /// Adds a relation between two IT-systems
        /// </summary>
        /// <param name="fromSystemUsageId">Source IT-system usage ID</param>
        /// <param name="toSystemUsageId">Destination IT-system usage ID</param>
        /// <param name="interfaceId">Optional interface id</param>
        /// <param name="description">Optional description</param>
        /// <param name="reference">Optional reference</param>
        /// <param name="frequencyId">Optional frequency id</param>
        /// <param name="contractId">Optional contract Id</param>
        /// <returns></returns>
        Result<SystemRelation, OperationError> AddRelation(int fromSystemUsageId, int toSystemUsageId, int? interfaceId, string description, string reference, int? frequencyId, int? contractId);
        /// <summary>
        /// Gets all relations FROM the target system usage
        /// </summary>
        /// <param name="systemUsageId"></param>
        /// <returns></returns>
        Result<IEnumerable<SystemRelation>, OperationError> GetRelationsFrom(int systemUsageId);
        /// <summary>
        /// Gets all relations TO the target system usage
        /// </summary>
        /// <param name="systemUsageId"></param>
        /// <returns></returns>
        Result<IEnumerable<SystemRelation>, OperationError> GetRelationsTo(int systemUsageId);
        /// <summary>
        /// Removes a system relation from the specified system usage
        /// </summary>
        /// <param name="fromSystemUsageId">Id of the "source" it system usage</param>
        /// <param name="relationId">Relation Id</param>
        /// <returns></returns>
        Result<SystemRelation, OperationFailure> RemoveRelation(int fromSystemUsageId, int relationId);
        /// <summary>
        /// Gets a system relation
        /// </summary>
        /// <param name="systemUsageId"></param>
        /// <param name="relationId"></param>
        /// <returns></returns>
        Result<SystemRelation, OperationFailure> GetRelationFrom(int systemUsageId, int relationId);
        /// <summary>
        /// Gets the systems which the "from" system can relate to
        /// </summary>
        /// <param name="fromSystemUsageId"></param>
        /// <param name="nameContent"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Result<IEnumerable<ItSystemUsage>, OperationError> GetSystemUsagesWhichCanBeRelatedTo(int fromSystemUsageId, Maybe<string> nameContent, int pageSize);
        /// <summary>
        /// Gets the valid options for the given system relation
        /// </summary>
        /// <param name="fromSystemUsageId"></param>
        /// <param name="toSystemUsageId"></param>
        /// <returns></returns>
        Result<RelationOptionsDTO, OperationError> GetAvailableOptions(int fromSystemUsageId, int toSystemUsageId);
        /// <summary>
        /// Edits a system relation
        /// </summary>
        /// <param name="fromSystemUsageId">Id of the "from" it system usage</param>
        /// <param name="relationId">Id of the specific relation in the "from" system usage</param>
        /// <param name="toSystemUsageId">Id of the "to" it system usage</param>
        /// <param name="description">Updated description</param>
        /// <param name="reference">Updated reference</param>
        /// <param name="toInterfaceId">Id of the specific "to" system usage interface to be used from the "from" system usage list of available interfaces</param>
        /// <param name="toContractId">Id of the specific "to" system usage contract to replace the existing system relation value</param>
        /// <param name="toFrequencyTypeId">Id of the specific "to" system frequency type to replace the existing system relation value</param>
        /// <returns></returns>
        Result<SystemRelation, OperationError> ModifyRelation(int fromSystemUsageId, int relationId, int toSystemUsageId, string description, string reference, int? toInterfaceId, int? toContractId, int? toFrequencyTypeId);
        /// <summary>
        /// Gets a list of relations which are associated with the provided contract
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns></returns>
        Result<IEnumerable<SystemRelation>, OperationError> GetRelationsAssociatedWithContract(int contractId);

        /// <summary>
        /// Gets a list of relations which are defined within the organization defined by <paramref name="organizationId"/>
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Result<IEnumerable<SystemRelation>, OperationError> GetRelationsDefinedInOrganization(int organizationId,
            int pageNumber, int pageSize);

        /// <summary>
        /// Adds information about which data sensitivity levels are applied to the system usage />
        /// </summary>
        /// <param name="itSystemUsageId"></param>
        /// <param name="sensitiveDataLevel"></param>
        /// <returns></returns>
        Result<ItSystemUsageSensitiveDataLevel, OperationError> AddSensitiveDataLevel(int itSystemUsageId, SensitiveDataLevel sensitiveDataLevel);

        /// <summary>
        /// Removes information about which data sensitivity levels are applied to the system usage />
        /// </summary>
        /// <param name="itSystemUsageId"></param>
        /// <param name="sensitiveDataLevel"></param>
        /// <returns></returns>
        Result<ItSystemUsageSensitiveDataLevel, OperationError> RemoveSensitiveDataLevel(int itSystemUsageId, SensitiveDataLevel sensitiveDataLevel);
    }
}