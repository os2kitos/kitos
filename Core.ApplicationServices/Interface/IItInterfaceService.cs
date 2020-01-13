using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Model.Result;

namespace Core.ApplicationServices.Interface
{
    public interface IItInterfaceService
    {
        TwoTrackResult<ItInterface,OperationFailure> Delete(int id);
    }
}
