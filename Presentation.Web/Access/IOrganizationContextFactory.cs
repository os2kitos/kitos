namespace Presentation.Web.Access
{
    public interface IOrganizationContextFactory
    {
        OrganizationContext CreateOrganizationContext(int organizationId);
        OrganizationAccessContext CreateOrganizationAccessContext(int organizationId);
    }
}