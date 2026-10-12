using Core.Abstractions.Types;
using Core.ApplicationServices.Model;
using Core.DomainModel;

namespace Core.ApplicationServices.Authorization
{
    public interface IAuthorizationModel
    {
        Result<bool, OperationError> AuthorizeUpdate(IEntityOwnedByOrganization entity,
            ISupplierAssociatedEntityUpdateParameters parameters);
        bool AuthorizeChildEntityDelete<TChild>(IEntityOwnedByOrganization parent, TChild child) where TChild : class;
    }
}
