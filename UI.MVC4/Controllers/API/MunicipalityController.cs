using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class MunicipalityController : GenericApiController<Municipality, int, MunicipalityApiModel>
    {
        private readonly IMunicipalityService _municipalityService;

        public MunicipalityController(IGenericRepository<Municipality> repository, IMunicipalityService municipalityService) : base(repository)
        {
            _municipalityService = municipalityService;
        }

        protected override Municipality PostQuery(Municipality item)
        {
            return _municipalityService.AddMunicipality(item);
        }
    }
}
