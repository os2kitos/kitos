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
    }
}