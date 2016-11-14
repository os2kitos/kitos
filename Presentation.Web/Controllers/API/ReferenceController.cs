using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;

namespace Presentation.Web.Controllers.API
{
    public class ReferenceController : GenericApiController<ExternalReference, ExternalReferenceDTO>
    {
        public ReferenceController(IGenericRepository<ExternalReference> repository) : base(repository)
        {
        }

      /*  public override HttpResponseMessage Post(ExternalReferenceDTO dto)
        {
            dto.ItProjectId
            return base.Post(dto);
        }*/
    }
}