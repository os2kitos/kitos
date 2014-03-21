using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class OrganizationService : IOrganizationService
    {
        public Organization CreateOrganization(string name)
        {
            return new Organization
            {
                Name = name,
                Config = DefaultConfig()
            };
        }

        public Organization CreateMunicipality(string name)
        {
            var organization = CreateOrganization(name);

            organization.OrgUnits.Add(new OrganizationUnit()
                {
                    Name = organization.Name,
                });
            
            return organization;
        }

        public bool IsUserMember(User user, Organization organization)
        {
            throw new System.NotImplementedException(); //TODO
        }

        private Config DefaultConfig()
        {
            return new Config()
            {
                ShowItContractModule = true,
                ShowItProjectModule = true,
                ShowItSystemModule = true,
                ItSupportModuleName_Id = 1,
                ItContractModuleName_Id = 1,
                ItProjectModuleName_Id = 1,
                ItSystemModuleName_Id = 1,
                ItSupportGuide = ".../itunderstøttelsesvejledning",
                ItProjectGuide = ".../itprojektvejledning",
                ItSystemGuide = ".../itsystemvejledning",
                ItContractGuide = ".../itkontraktvejledning",
                ShowBC = true,
                ShowPortfolio = true,
                ShowColumnMandatory = true,
                ShowColumnTechnology = true,
                ShowColumnUsage = true,
                ShowTabOverview = true
            };
        }
    }
}