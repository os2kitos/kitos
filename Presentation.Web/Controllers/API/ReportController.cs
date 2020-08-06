using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.Reports;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class ReportController : GenericApiController<Report, NamedEntityDTO>
    {
        public ReportController(IGenericRepository<Report> repository)
            : base(repository)
        {
        }

        [NonAction]
        public override HttpResponseMessage Post(int organizationId, NamedEntityDTO dto) => throw new NotSupportedException();

        [NonAction]
        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj) => throw new NotSupportedException();

        [NonAction]
        public override HttpResponseMessage Delete(int id, int organizationId) => throw new NotSupportedException();
    }
}