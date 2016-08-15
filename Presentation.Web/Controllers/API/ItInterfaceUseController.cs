using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AutoMapper;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    /// <remarks>
    /// cannot derive from GenericApiController as this entity uses a composite key
    /// </remarks>>
    public class ItInterfaceUseController : BaseApiController
    {
        private readonly IGenericRepository<ItInterfaceUse> _repository;

        public ItInterfaceUseController(IGenericRepository<ItInterfaceUse> repository)
        {
            _repository = repository;
        }

        public HttpResponseMessage GetInterfaceBySystem(int sysId, int orgId, bool? interfaces)
        {
            try
            {
                var items = _repository.Get(x => x.ItSystemId == sysId && (x.ItInterface.OrganizationId == orgId || x.ItInterface.AccessModifier == AccessModifier.Public));
                var intfs = items.Select(x => x.ItInterface);
                var dtos = Mapper.Map<IEnumerable<ItInterfaceDTO>>(intfs);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetBySystem(int sysId, int orgId, string q)
        {
            try
            {
                var items = _repository.Get(x => x.ItSystemId == sysId && (x.ItInterface.OrganizationId == orgId || x.ItInterface.AccessModifier == AccessModifier.Public) && x.ItInterface.Name.Contains(q));
                var dtos = Map<IEnumerable<ItInterfaceUse>, IEnumerable<ItInterfaceUseDTO>>(items);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
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
                return LogError(e);
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
                return LogError(e);
            }
        }
    }
}
