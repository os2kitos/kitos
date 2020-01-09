using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using AutoMapper;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class ExhibitController : GenericContextAwareApiController<ItInterfaceExhibit, ItInterfaceExhibitDTO>
    {
        private readonly IGenericRepository<ItInterfaceExhibit> _repository;
        private readonly IGenericRepository<ItInterface> _interfaceRepository;

        public ExhibitController(IGenericRepository<ItInterfaceExhibit> repository, IAuthorizationContext authorizationContext, IGenericRepository<ItInterface> interfaceRepository)
            : base(repository, authorizationContext)
        {
            _repository = repository;
            _interfaceRepository = interfaceRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceDTO>>))]
        public HttpResponseMessage GetInterfacesBySystem(int sysId, int orgId, bool? interfaces)
        {
            try
            {
                var exhibits = _repository.Get(x => x.ItSystemId == sysId && (x.ItInterface.OrganizationId == orgId || x.ItInterface.AccessModifier == AccessModifier.Public));
                var intfs = exhibits.Select(x => x.ItInterface);
                var dtos = Mapper.Map<IEnumerable<ItInterfaceDTO>>(intfs.Where(AllowRead));
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceExhibitDTO>>))]
        public HttpResponseMessage GetBySystem(int sysId, int orgId, string q)
        {
            try
            {
                var exhibit = _repository.Get(x => x.ItSystemId == sysId && (x.ItInterface.OrganizationId == orgId || x.ItInterface.AccessModifier == AccessModifier.Public) && x.ItInterface.Name.Contains(q));
                var dtos = Map(exhibit.Where(AllowRead));
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItInterfaceExhibit>(x => _interfaceRepository.AsQueryable().ById(x.Id), base.GetCrudAuthorization());
        }
    }
}
