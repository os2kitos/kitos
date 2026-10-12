using Core.Abstractions.Types;
using Core.ApplicationServices.Model;
using Core.DomainModel;

namespace Core.ApplicationServices.Authorization;

public class CrudAuthorizationModel : IAuthorizationModel
{
    private readonly IAuthorizationContext _authorizationContext;

    public CrudAuthorizationModel(IAuthorizationContext authorizationContext)
    {
        _authorizationContext = authorizationContext;
    }

    public Result<bool, OperationError> AuthorizeUpdate(IEntityOwnedByOrganization entity,
        ISupplierAssociatedEntityUpdateParameters parameters)
    {
        return _authorizationContext.AllowModify(entity);
    }

    public bool AuthorizeChildEntityDelete<TChild>(IEntityOwnedByOrganization parent, TChild child) where TChild : class
    {
        return _authorizationContext.AllowModify(parent);
    }
}