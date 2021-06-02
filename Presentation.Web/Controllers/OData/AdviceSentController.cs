using System.Linq;
using System.Web.Http;
using Core.ApplicationServices;
using Microsoft.AspNet.OData;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    public class AdviceSentController : BaseOdataController
    {
        private readonly IAdviceService _adviceService;

        public AdviceSentController(IAdviceService adviceService)
        {
            _adviceService = adviceService;
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            var sentFromAll = _adviceService
                .GetAccessibleCurrentToUser()
                .SelectMany(x => x.AdviceSent);

            return Ok(sentFromAll);
        }
    }
}