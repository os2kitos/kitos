using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemController : GenericApiController<ItSystem, int, ItSystemDTO>
    {
        private readonly IItSystemService _systemService;

        public ItSystemController(IGenericRepository<ItSystem> repository, IItSystemService systemService) : base(repository)
        {
            _systemService = systemService;
        }

        public HttpResponseMessage GetAllInterfaces(bool? interfaces)
        {
            try
            {
                var systems = _systemService.GetAllInterfaces(null);
                var dtos = Map(systems);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetAllNonInterfaces(bool? nonInterfaces)
        {
            try
            {
                var systems = _systemService.GetAllNonInterfaces(null);
                var dtos = Map(systems);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
