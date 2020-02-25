﻿using System.Net;
using System.Net.Http;
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

        public override HttpResponseMessage Post(NamedEntityDTO dto)
        {
            return CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            return CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        public override HttpResponseMessage Delete(int id, int organizationId)
        {
            return CreateResponse(HttpStatusCode.MethodNotAllowed);
        }
    }
}