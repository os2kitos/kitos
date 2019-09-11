using Core.ApplicationServices.Model.Result;

namespace Core.ApplicationServices.Interface.ExhibitUsage
{
    public interface IInterfaceExhibitUsageService
    {
        OperationResult Delete(int systemUsageId, int interfaceExhibitId);
    }
}
