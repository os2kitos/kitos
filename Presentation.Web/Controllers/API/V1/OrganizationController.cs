using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Web;
using System.Web.Http;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Queries.Organization;
using Core.DomainServices.Queries;
using Newtonsoft.Json.Linq;
using Presentation.Web.Controllers.API.V1.Mapping;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.Generic.Queries;

namespace Presentation.Web.Controllers.API.V1
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
        
        public virtual HttpResponseMessage Get([FromUri] string q, [FromUri] V1BoundedPaginationQuery paging)
        {
            q = HttpUtility.UrlDecode(q);
            var refinements = new List<IDomainQuery<Organization>>();

            if (!string.IsNullOrWhiteSpace(q))
                refinements.Add(new QueryByNameOrCvrContent(q));

            return _organizationService
                .SearchAccessibleOrganizations(false, refinements.ToArray())
                .OrderBy(x => x.Name).ThenBy(x => x.Id)
                .Page(paging)
                .ToList()
                .MapToShallowOrganizationDTOs()
                .ToList()
                .Transform(Ok);
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
            if (obj.TryGetValue("cvr", out var jtoken))
            {
                var cvr = jtoken.Value<string>();

                if (!string.Equals(cvr, organization.Cvr))
                {
                    var canEdit = _organizationService
                        .CanActiveUserModifyCvr(organization.Uuid)
                        .Match(canEdit => canEdit, _ => false);

                    if (!canEdit)
                    {
                        return Forbidden();
                    }
                }
            }

            return base.Patch(id, organizationId, obj);
        }

        [NonAction]
        public override HttpResponseMessage Delete(int id, int organizationId) => throw new NotSupportedException();
    }
}
