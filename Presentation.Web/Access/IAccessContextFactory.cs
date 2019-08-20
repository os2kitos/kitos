namespace Presentation.Web.Access
{
    public interface IAccessContextFactory
    {
        IAccessContext CreateOrganizationAccessContext();
    }
}