using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class EconomyYearController : GenericApiController<EconomyYear, int, EconomyYearDTO>
    {
        private readonly IItProjectService _itProjectService;

        public EconomyYearController(IGenericRepository<EconomyYear> repository, IItProjectService itProjectService) : base(repository)
        {
            _itProjectService = itProjectService;
        }

        protected override EconomyYear PatchQuery(EconomyYear item)
        {
            CheckHasWriteAccess();

            return base.PatchQuery(item);
        }

        private void CheckHasWriteAccess()
        {
            //TODO
        }
    }
}
