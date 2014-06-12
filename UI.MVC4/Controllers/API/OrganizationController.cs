using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class OrganizationController : GenericHasRightsController<Organization, AdminRight, AdminRole, OrganizationDTO>
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IGenericRepository<Organization> repository, IGenericRepository<AdminRight> rightRepository, IOrganizationService organizationService) 
            : base(repository, rightRepository)
        {
            _organizationService = organizationService;
        }

        public HttpResponseMessage GetBySearch(string q)
        {
            try
            {
                var orgs = Repository.Get(org => org.Name.Contains(q));
                return Ok(Map(orgs));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetPublic(string q, [FromUri] bool? @public)
        {
            try
            {
                var orgs = Repository.Get(org => org.AccessModifier == AccessModifier.Public && (org.Name.Contains(q) || org.Cvr.Contains(q)));
                return Ok(Map(orgs));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        protected override Organization PostQuery(Organization item)
        {
            _organizationService.AddDefaultOrgUnit(item);
            return base.PostQuery(item);
        }
        
        public virtual HttpResponseMessage GetAllRights(bool? rights)
        {
            try
            {
                if (!IsGlobalAdmin()) return Unauthorized();
                var theRights = RightRepository.Get();
                var dtos = Map<IEnumerable<AdminRight>, IEnumerable<RightOutputDTO>>(theRights);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
