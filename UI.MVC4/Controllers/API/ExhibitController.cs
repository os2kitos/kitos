using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AutoMapper;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ExhibitController : GenericApiController<ItInterfaceExhibit, ItInterfaceExhibitDTO>
    {
        private readonly IGenericRepository<ItInterfaceExhibit> _repository;

        public ExhibitController(IGenericRepository<ItInterfaceExhibit> repository) 
            : base(repository)
        {
            _repository = repository;
        }

        public HttpResponseMessage GetInterfacesBySystem(int sysId)
        {
            try
            {
                var exhibits = _repository.Get(x => x.ItSystemId == sysId);
                var interfaces = exhibits.Select(x => x.ItInterface);
                var dtos = Mapper.Map<IEnumerable<ItInterfaceDTO>>(interfaces);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
