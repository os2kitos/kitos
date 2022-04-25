
using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainServices.Repositories.Organization;
using Serilog;

namespace Core.DomainServices.Context
{
    public class DefaultOrganizationResolver : IDefaultOrganizationResolver
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ILogger _logger;

        public DefaultOrganizationResolver(IOrganizationRepository organizationRepository, ILogger logger)
        {
            _organizationRepository = organizationRepository;
            _logger = logger;
        }

        public Organization Resolve()
        {
            var organizations = _organizationRepository.GetAll().Where(x => x.IsDefaultOrganization == true).ToList();
            if (organizations.Count != 1)
            {
                _logger.Error("Expected 1 default organization but found {defaultOrgCount} with ids {idcsv}", organizations.Count, string.Join(",", organizations.Select(x => x.Id)));
            }
            return organizations.Single();
        }
    }
}
