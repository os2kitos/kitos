using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Interface.ExhibitUsage
{
    public interface IInterfaceExhibitUsageService
    {
        Result<OperationResult, object> DeleteByKey(object[] key);
    }
}
