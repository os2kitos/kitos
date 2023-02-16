using System.Linq;
using System.Web.Http;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData
{
    /// <summary>
    /// Gives access to relations between ItContract and ElementTypes
    /// Primarily used for reporting
    /// </summary>
    [InternalApi]
    public class ItContractAgreementElementTypesController : BaseController<ItContractAgreementElementTypes>
    {
        public ItContractAgreementElementTypesController(IGenericRepository<ItContractAgreementElementTypes> repository)
            : base(repository)
        {

        }

        public override IHttpActionResult Get()
        {
            var accessLevel = AuthorizationContext.GetCrossOrganizationReadAccess();
            var query = Repository.AsQueryable();

            if (accessLevel < CrossOrganizationDataReadAccessLevel.All)
            {
                var orgIds = UserContext.OrganizationIds.ToList();
                query = query.Where(x => orgIds.Contains(x.ItContract.OrganizationId));
            }

            return Ok(query);
        }

        public override IHttpActionResult Get(int key)
        {
            var agreementElementTypes = Repository.GetByKey(key);
            if (agreementElementTypes == null)
                return NotFound();
            if (!AuthorizationContext.AllowReads(agreementElementTypes.ItContract))
                return Forbidden();

            return Ok(agreementElementTypes);
        }
    }
}
