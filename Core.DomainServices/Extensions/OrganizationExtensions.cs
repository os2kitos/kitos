using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.Organization;

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

        public static IQueryable<Organization> ByPartOfNameOrCvr(this IQueryable<Organization> result, string query)
        {
            return new QueryByNameOrCvrContent(query).Apply(result);
        }
    }
}
