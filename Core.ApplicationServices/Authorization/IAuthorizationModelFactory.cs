namespace Core.ApplicationServices.Authorization
{
    public interface IAuthorizationModelFactory
    {
        CrudAuthorizationModel CreateCrudAuthorizationModel(IAuthorizationContext authorizationContext);
        FieldAuthorizationModel CreateFieldAuthorizationModel(IAuthorizationContext authorizationContext);
    }
}
