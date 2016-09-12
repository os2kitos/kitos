using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class OrganizationController : GenericContextAwareApiController<Organization, OrganizationDTO>
    {
        private readonly IOrganizationService _organizationService;
        private readonly IGenericRepository<User> _useRepository;

        public OrganizationController(IGenericRepository<Organization> repository, IOrganizationService organizationService, IGenericRepository<User> useRepository)
            : base(repository)
        {
            _organizationService = organizationService;
            _useRepository = useRepository;
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
                        // filter by project name or cvr
                        (org.Name.Contains(q) || org.Cvr.Contains(q))).ToList();

                // filter locally
                var orgs2 = orgs.Where(org => KitosUser.IsGlobalAdmin || org.ObjectOwnerId == KitosUser.Id ||
                    // it's public everyone can see it
                        org.AccessModifier == AccessModifier.Public ||
                    // everyone in the same organization can see normal objects
                        org.AccessModifier == AccessModifier.Local &&
                        org.Id == orgId || org.OrgUnits.Any(x => x.Rights.Any(y => y.UserId == KitosUser.Id)));

                var dtos = Map(orgs2);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetPublic(string q, [FromUri] bool? @public, int orgId)
        {
            try
            {
                //var orgs = Repository.Get(
                //    org =>
                //        // filter by project name or cvr
                //        (org.Name.Contains(q) || org.Cvr.Contains(q)) &&
                //        // global admin sees all
                //        (KitosUser.IsGlobalAdmin ||
                //        // object owner sees his own objects
                //        org.ObjectOwnerId == KitosUser.Id ||
                //        // it's public everyone can see it
                //        org.AccessModifier == AccessModifier.Public ||
                //        // everyone in the same organization can see normal objects
                //        org.AccessModifier == AccessModifier.Local &&
                //        org.Id == orgId ||
                //        // user with a role on the object can see it
                //        org.Rights.Any(x => x.UserId == KitosUser.Id) ||
                //        // !SPECIAL CASE! user with a role on a org unit can see it
                //        org.OrgUnits.Any(x => x.Rights.Any(y => y.UserId == KitosUser.Id)))
                //    );

                // MySql.Data v6.9.5 can't do org.OrgUnits.Any(x => x.Rights.Any(y => y.UserId == KitosUser.Id))
                // so have to do it the slow way :(

                var orgs = Repository.Get(
                    org =>
                        // filter by project name or cvr
                        (org.Name.Contains(q) || org.Cvr.Contains(q))).ToList();

                // filter locally
                var orgs2 = orgs.Where(org => KitosUser.IsGlobalAdmin || org.ObjectOwnerId == KitosUser.Id ||
                        // it's public everyone can see it
                        org.AccessModifier == AccessModifier.Public ||
                        // everyone in the same organization can see normal objects
                        org.AccessModifier == AccessModifier.Local &&
                        org.Id == orgId || org.OrgUnits.Any(x => x.Rights.Any(y => y.UserId == KitosUser.Id)));

                var dtos = Map(orgs2);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
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
                // OLD METHOD
                // gets users with a role in the organization
                //var qry =
                //    Repository.AsQueryable().Single(x => x.Id == id).OrgUnits.SelectMany(y => y.Rights)
                //              .Select(z => z.User)
                //              .Where(u => u.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) != -1 || u.Email.IndexOf(q, StringComparison.OrdinalIgnoreCase) != -1);

                //var qry = _useRepository.Get(x => x.CreatedInId == id && (x.Name.Contains(q) || x.Email.Contains(q)));
                var qry =
                    _useRepository.Get(
                        u =>
                            u.OrganizationRights.Count(r => r.OrganizationId == id) != 0 && (u.Name.Contains(q) || u.Email.Contains(q)));

                return Ok(Map<IEnumerable<User>, IEnumerable<UserDTO>>(qry));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override Organization PostQuery(Organization item)
        {
            _organizationService.SetupDefaultOrganization(item, KitosUser);
            return base.PostQuery(item);
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            if (!KitosUser.IsGlobalAdmin)
            {
	            if (obj.GetValue("typeId", StringComparison.InvariantCultureIgnoreCase) != null)
                {
                    // only global admin is allowed to change the type of an organization
                    return Unauthorized();
                }
            }

            return base.Patch(id, organizationId, obj);
        }
    }
}
