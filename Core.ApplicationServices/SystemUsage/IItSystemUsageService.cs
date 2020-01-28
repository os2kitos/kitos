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
        /// <param name="linkName">Optional link name</param>
        /// <param name="linkUrl">Optional link url</param>
        /// <param name="frequencyId">Optional frequency id</param>
        /// <param name="contractId">Optional contract Id</param>
        /// <returns></returns>
        Result<SystemRelation, OperationError> AddRelation(int sourceId, int destinationId, int? interfaceId, string description, string linkName, string linkUrl, int? frequencyId, int? contractId);
    }
}