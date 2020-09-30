using Core.DomainModel;
using Core.DomainModel.Result;

namespace Core.DomainServices.Options
{
    public interface IMultipleOptionTypeInstancesAssignmentService<in TOwner, TOption> 
        where TOption : OptionEntity<TOwner>
        where TOwner: IOwnedByOrganization
    {
        Result<TOption, OperationError> Assign(TOwner owner, int optionId);
        Result<TOption, OperationError> Remove(TOwner owner, int optionId);
    }
}
