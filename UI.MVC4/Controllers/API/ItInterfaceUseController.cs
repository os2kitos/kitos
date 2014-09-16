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
    public class ItInterfaceUseController : BaseApiController
    {
        private readonly IGenericRepository<ItInterfaceUse> _repository;

        public ItInterfaceUseController(IGenericRepository<ItInterfaceUse> repository)
        {
            _repository = repository;
        }

        public HttpResponseMessage GetBySystem(int sysId)
        {
            try
            {
                var items = _repository.Get(x => x.ItSystemId == sysId);
                var interfaces = items.Select(x => x.ItInterface);
                var dtos = Mapper.Map<IEnumerable<ItInterfaceDTO>>(interfaces);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Post(int sysId, int interfaceId)
        {
            try
            {
                var item = new ItInterfaceUse {ItSystemId = sysId, ItInterfaceId = interfaceId};
                _repository.Insert(item);
                _repository.Save();
                
                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Delete(int sysId, int interfaceId)
        {
            try
            {
                _repository.DeleteByKey(new object[] {sysId, interfaceId});
                _repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
