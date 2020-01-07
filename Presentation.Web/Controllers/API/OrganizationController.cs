using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    [ControllerEvaluationCompleted]
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

        [DeprecatedApi]
        public virtual HttpResponseMessage Get([FromUri] string q, [FromUri] PagingModel<Organization> paging)
        {
            if (!string.IsNullOrWhiteSpace(q))
                paging.Where(x => x.Name.Contains(q) || x.Cvr.Contains(q));
            return GetAll(paging);
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
        [DeprecatedApi]
        public HttpResponseMessage GetUsers(int id, string q, bool? users)
        {
            try
            {
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
            if (item.TypeId > 0)
            {
                var typeKey = (OrganizationTypeKeys)item.TypeId;
                switch (typeKey)
                {
                    case OrganizationTypeKeys.Kommune:
                        if (!FeatureChecker.CanExecute(KitosUser, Feature.CanSetOrganizationTypeKommune))
                            throw new SecurityException();
                        break;
                    case OrganizationTypeKeys.Interessefællesskab:
                        if (!FeatureChecker.CanExecute(KitosUser, Feature.CanSetOrganizationTypeInteressefællesskab))
                            throw new SecurityException();
                        break;
                    case OrganizationTypeKeys.Virksomhed:
                        if (!FeatureChecker.CanExecute(KitosUser, Feature.CanSetOrganizationTypeVirksomhed))
                            throw new SecurityException();
                        break;
                    case OrganizationTypeKeys.AndenOffentligMyndighed:
                        if (!FeatureChecker.CanExecute(KitosUser, Feature.CanSetOrganizationTypeAndenOffentligMyndighed))
                            throw new SecurityException();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (item.AccessModifier == AccessModifier.Public &&
                !FeatureChecker.CanExecute(KitosUser, Feature.CanSetAccessModifierToPublic))
                throw new SecurityException("Du har ikke rettigheder til at sætte synligheden til offentlig");

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
                    return Forbidden();
                }
            }

            return base.Patch(id, organizationId, obj);
        }

        protected override bool HasWriteAccess(Organization obj, User user, int organizationId)
        {
            //if readonly
            if (user.IsReadOnly && !user.IsGlobalAdmin)
                return false;
            // local admin have write access if the obj is in context
            if (obj.IsInContext(organizationId) &&
                user.OrganizationRights.Any(x => x.OrganizationId == organizationId && (x.Role == OrganizationRole.LocalAdmin || x.Role == OrganizationRole.OrganizationModuleAdmin)))
                return true;

            return base.HasWriteAccess(obj, user, organizationId);
        }
    }
}
