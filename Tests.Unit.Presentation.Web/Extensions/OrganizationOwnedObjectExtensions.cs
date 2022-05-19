using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Tests.Unit.Presentation.Web.Extensions
{
    public static class OrganizationOwnedObjectExtensions
    {
        public static T InOrganization<T>(this T ownedObject, Organization organization) where T : IOwnedByOrganization
        {
            ownedObject.Organization = organization;
            ownedObject.OrganizationId = organization.Id;
            return ownedObject;
        }
    }
}
