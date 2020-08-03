using System.Web.Http;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [ODataRoutePrefix("LocalPaymentFrequencyTypes")]
    public class LocalPaymentFrequencyTypesController : LocalOptionBaseController<LocalPaymentFreqencyType, ItContract, PaymentFreqencyType>
    {
        public LocalPaymentFrequencyTypesController(IGenericRepository<LocalPaymentFreqencyType> repository, IGenericRepository<PaymentFreqencyType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }

        [EnableQuery]
        [ODataRoute]
        public override IHttpActionResult GetByOrganizationId(int organizationId) => base.GetByOrganizationId(organizationId);

        [EnableQuery]
        [ODataRoute]
        public override IHttpActionResult Get(int organizationId, int key) => base.Get(organizationId, key);

        [ODataRoute]
        public override IHttpActionResult Post(int organizationId, LocalPaymentFreqencyType entity) => base.Post(organizationId, entity);

        [ODataRoute]
        public override IHttpActionResult Patch(int organizationId, int key, Delta<LocalPaymentFreqencyType> delta) => base.Patch(organizationId, key, delta);

        [ODataRoute]
        public override IHttpActionResult Delete(int organizationId, int key) => base.Delete(organizationId, key);
    }
}
