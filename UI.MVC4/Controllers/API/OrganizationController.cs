using System;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class OrganizationController : GenericApiController<Organization, OrganizationDTO, OrganizationDTO>
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IGenericRepository<Organization> repository, IOrganizationService organizationService) 
            : base(repository)
        {
            _organizationService = organizationService;
        }

        public HttpResponseMessage GetBySearch(string q)
        {
            try
            {
                var orgs = Repository.Get(org => org.Name.StartsWith(q));
                return Ok(Map(orgs));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetCompanies([FromUri] bool? company)
        {
            try
            {
                var orgs = Repository.Get(org => org.Type == OrganizationType.Company);
                return Ok(Map(orgs));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        protected override Organization PostQuery(Organization item)
        {
            item = _organizationService.CreateOrganization(item.Name, OrganizationType.Municipality, KitosUser);
            return base.PostQuery(item);
        }
    }
}
