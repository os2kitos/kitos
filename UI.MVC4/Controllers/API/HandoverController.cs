using System;
using System.Net.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class HandoverController : GenericApiController<Handover, int, HandoverDTO>
    {
        public HandoverController(IGenericRepository<Handover> repository)
            : base(repository)
        {
        }
    }
}
