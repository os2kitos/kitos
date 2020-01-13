using Core.ApplicationServices.Model.Result;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Model.Result;

namespace Core.ApplicationServices.SystemUsage
{
    public interface IItSystemUsageService
    {
        TwoTrackResult<ItSystemUsage, GenericOperationFailure> Add(ItSystemUsage usage, User objectOwner);
        TwoTrackResult<ItSystemUsage, GenericOperationFailure> Delete(int id);
        ItSystemUsage GetByOrganizationAndSystemId(int organizationId, int systemId);
        ItSystemUsage GetById(int usageId);
    }
}