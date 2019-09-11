using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Interface.ExhibitUsage
{
    public interface IInterfaceExhibitUsageService
    {
        OperationResult DeleteByKey(int systemUsageId, int interfaceExhibitId);
    }
}
