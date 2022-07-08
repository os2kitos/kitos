using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [Authorize]
    [PublicApi]
    public class EconomyStreamsController : BaseEntityController<EconomyStream>
    {
        private readonly IGenericRepository<EconomyStream> _repository;

        public EconomyStreamsController(IGenericRepository<EconomyStream> repository)
            : base(repository)
        {
            _repository = repository;
        }

        [NonAction]
        public override IHttpActionResult Post(int organizationId, EconomyStream entity) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Patch(int key, Delta<EconomyStream> delta) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Delete(int key) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Get() => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Get(int key) => throw new NotSupportedException();

        // GET /Organizations(1)/ItContracts
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All & ~AllowedQueryOptions.Expand)]
        [ODataRoute("ExternEconomyStreams(Organization={orgKey})")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<EconomyStream>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetByOrganization(int orgKey)
        {
            var accessLevel = GetOrganizationReadAccessLevel(orgKey);

            if (accessLevel < OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }

            var result =
                _repository.AsQueryable()
                    .Where(
                        m =>
                            m.ExternPaymentFor.OrganizationId == orgKey &&
                            m.InternPaymentFor == null);

            return Ok(result);
        }
    }
}
