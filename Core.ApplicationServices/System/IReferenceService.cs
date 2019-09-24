using Core.ApplicationServices.Model.Result;

namespace Core.ApplicationServices.System
{
    public interface IReferenceService
    {
        OperationResult Delete(int systemId);
    }
}
