using Core.DomainModel.Organization;

namespace Core.DomainServices.Extensions
{
    public static class OrganizationExtensions
    {
        public static bool IsCvrInvalid(this Organization organization)
        {
            //Cvr is optional
            var isCvrProvided = string.IsNullOrWhiteSpace(organization.Cvr) == false;

            //Checking if the provided cvr is valid
            return isCvrProvided && (organization.Cvr.Length > 10 || organization.Cvr.Length < 8);
        }
    }
}
