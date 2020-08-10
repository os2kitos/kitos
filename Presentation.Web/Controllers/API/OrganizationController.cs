using System;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Newtonsoft.Json.Linq;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class OrganizationController : GenericApiController<Organization, OrganizationDTO>
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(
            IGenericRepository<Organization> repository,
            IOrganizationService organizationService)
            : base(repository)
        {
            _organizationService = organizationService;
        }

        public virtual HttpResponseMessage Get([FromUri] string q, [FromUri] PagingModel<Organization> paging)
        {
            if (!string.IsNullOrWhiteSpace(q))
                paging.Where(x => x.Name.Contains(q) || x.Cvr.Contains(q));
            return GetAll(paging);
        }

        public HttpResponseMessage GetBySearch(string q, int orgId, int take = 25)
        {
            try
            {
                var canSeeAll = GetCrossOrganizationReadAccessLevel() == CrossOrganizationDataReadAccessLevel.All;
                var userId = UserId;
                var dtos =
                    Repository
                        .AsQueryable()
                        .Where(org => org.Name.Contains(q) || org.Cvr.Contains(q))
                        .Where(org => canSeeAll || org.ObjectOwnerId == userId ||
                                      // it's public everyone can see it
                                      org.AccessModifier == AccessModifier.Public ||
                                      // everyone in the same organization can see normal objects
                                      org.AccessModifier == AccessModifier.Local &&
                                      org.Id == orgId ||
                                      org.OrgUnits.Any(x => x.Rights.Any(y => y.UserId == userId)))
                        .AsEnumerable()
                        .Where(AllowRead)
                        .OrderBy(_ => _.Name)
                        .Take(take)
                        .MapToNamedEntityDTOs()
                        .ToList();

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override bool AllowCreate<T>(int organizationId, IEntity entity)
        {
            return AuthorizationContext.AllowCreate<Organization>(organizationId);
        }

        protected override Organization PostQuery(Organization item)
        {
            var result = _organizationService.CreateNewOrganization(item);
            if (result.Ok)
            {
                return result.Value;
            }

            if (result.Error == OperationFailure.Forbidden)
            {
                throw new SecurityException();
            }

            throw new InvalidOperationException(result.Error.ToString("G"));
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            var organization = Repository.GetByKey(id);
            if (organization == null)
            {
                return NotFound();
            }

            var token = obj.GetValue("typeId", StringComparison.InvariantCultureIgnoreCase);
            if (token != null)

            {
                var typeId = token.Value<int>();
                if (typeId > 0)
                {
                    if (!_organizationService.CanChangeOrganizationType(organization, (OrganizationTypeKeys)typeId))
                    {
                        return Forbidden();
                    }
                }
            }

            return base.Patch(id, organizationId, obj);
        }
    }
}
