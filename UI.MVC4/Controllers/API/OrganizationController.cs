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

        public virtual HttpResponseMessage Get([FromUri] string q, [FromUri] PagingModel<Organization> paging)
        {
            if (!string.IsNullOrWhiteSpace(q))
                paging.Where(x => x.Name.Contains(q) || x.Cvr.Contains(q));
            return base.GetAll(paging);
        }

        public HttpResponseMessage GetBySearch(string q, int orgId)
        {
            try
            {
                var orgs = Repository.Get(
                    org => 
                        // filter by project name
                        org.Name.Contains(q) &&
                        // global admin sees all within the context
                        (KitosUser.IsGlobalAdmin && org.Id == orgId ||
                        // object owner sees his own objects
                        org.ObjectOwnerId == KitosUser.Id ||
                        // it's public everyone can see it
                        org.AccessModifier == AccessModifier.Public ||
                        // everyone in the same organization can see normal objects
                        org.AccessModifier == AccessModifier.Normal &&
                        org.Id == orgId ||
                        // only users with a role on the object can see private objects
                        org.AccessModifier == AccessModifier.Private && org.Rights.Any(x => x.UserId == KitosUser.Id))
                    );
                var dtos = Map(orgs);
                return Ok(dtos);
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
                var orgs = Repository.Get(
                    org => 
                        // filter by project name or cvr
                        org.Name.Contains(q) || org.Cvr.Contains(q) &&
                        // global admin sees all within the context
                        (KitosUser.IsGlobalAdmin && org.Id == orgId ||
                        // object owner sees his own objects
                        org.ObjectOwnerId == KitosUser.Id ||
                        // it's public everyone can see it
                        org.AccessModifier == AccessModifier.Public ||
                        // everyone in the same organization can see normal objects
                        org.AccessModifier == AccessModifier.Normal &&
                        org.Id == orgId ||
                        // only users with a role on the object can see private objects
                        org.AccessModifier == AccessModifier.Private && org.Rights.Any(x => x.UserId == KitosUser.Id))
                    );
                
                var dtos = Map(orgs);
                return Ok(dtos);
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
                              .Where(u => u.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) != -1 || u.Email.IndexOf(q, StringComparison.OrdinalIgnoreCase) != -1);

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
