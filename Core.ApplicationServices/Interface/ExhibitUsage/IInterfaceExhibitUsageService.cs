using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.Interface.ExhibitUsage
{
    public interface IInterfaceExhibitUsageService
    {
        Result<ItInterfaceExhibitUsage, OperationFailure> Delete(int systemUsageId, int interfaceExhibitId);
    }
}
