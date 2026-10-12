namespace Core.ApplicationServices.Authorization;

public class AuthorizationModelFactory : IAuthorizationModelFactory
{
    private readonly IOrganizationalUserContext _userContext;
    private readonly ISupplierAssociatedFieldsService _supplierAssociatedFieldsService;

    public AuthorizationModelFactory(IOrganizationalUserContext userContext, ISupplierAssociatedFieldsService supplierAssociatedFieldsService)
    {
        _userContext = userContext;
        _supplierAssociatedFieldsService = supplierAssociatedFieldsService;
    }
    public CrudAuthorizationModel CreateCrudAuthorizationModel(IAuthorizationContext authorizationContext)
    {
        return new CrudAuthorizationModel(authorizationContext);
    }

    public FieldAuthorizationModel CreateFieldAuthorizationModel(IAuthorizationContext authorizationContext)
    {
        return new FieldAuthorizationModel(_userContext, _supplierAssociatedFieldsService, authorizationContext);
    }
}