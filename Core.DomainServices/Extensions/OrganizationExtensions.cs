using Core.DomainModel.Organization;

namespace Core.DomainServices.Extensions
{
    public static class OrganizationExtensions
    {
        public static bool IsCvrInvalid(this Organization organization)
        {
            //Cvr is optional
            var isCvrProvided = string.IsNullOrWhiteSpace(organization.Cvr) == false;

            //If cvr is defined, it must be valid
            return isCvrProvided && (organization.Cvr.Length > 10 || organization.Cvr.Length < 8);
        }
    }
}
