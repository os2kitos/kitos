namespace Presentation.Web.Access
{
    public interface IOrganizationContextFactory
    {
        OrganizationContext CreateOrganizationContext(int organizationId);
        IAccessContext CreateOrganizationAccessContext();
    }
}