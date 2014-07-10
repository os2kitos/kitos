using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class OrganizationController : GenericApiController<Organization, OrganizationDTO>
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
                var orgs = Repository.Get(org => org.Name.Contains(q));
                return Ok(Map(orgs));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetPublic(string q, [FromUri] bool? @public, int orgId)
        {
            try
            {
                var orgs = Repository.Get(org => (org.AccessModifier == AccessModifier.Public || org.Id == orgId) && (org.Name.Contains(q) || org.Cvr.Contains(q)));
                return Ok(Map(orgs));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// Gets all users from an organization matching the search criteria.
        /// </summary>
        /// <param name="q">Text search string</param>
        /// <param name="id">Organization id</param>
        /// <param name="users">Route identifier</param>
        /// <returns>All users from organization <see cref="id"/> which matched the search criteria <see cref="q"/></returns>
        public HttpResponseMessage GetUsers(int id, string q, bool? users)
        {
            try
            {
                var qry =
                    Repository.AsQueryable().Single(x => x.Id == id).OrgUnits.SelectMany(y => y.Rights)
                              .Select(z => z.User)
                              .Where(u => u.Name.Contains(q) || u.Email.Contains(q));

                return Ok(Map<IEnumerable<User>, IEnumerable<UserDTO>>(qry));
            }
            catch (Exception e)
            {
                return Error(e);
            }            
        }

        protected override Organization PostQuery(Organization item)
        {
            _organizationService.SetupDefaultOrganization(item, KitosUser);
            return base.PostQuery(item);
        }
    }
}
