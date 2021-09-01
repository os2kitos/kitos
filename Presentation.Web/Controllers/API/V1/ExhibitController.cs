using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Interface;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Infrastructure.Services.Types;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;
using Presentation.Web.Models.API.V1;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    public class ExhibitController : GenericApiController<ItInterfaceExhibit, ItInterfaceExhibitDTO>
    {
        private readonly IGenericRepository<ItInterfaceExhibit> _repository;
        private readonly IGenericRepository<ItInterface> _interfaceRepository;
        private readonly IItInterfaceService _interfaceService;

        public ExhibitController(
            IGenericRepository<ItInterfaceExhibit> repository,
            IGenericRepository<ItInterface> interfaceRepository,
            IItInterfaceService interfaceService)
            : base(repository)
        {
            _repository = repository;
            _interfaceRepository = interfaceRepository;
            _interfaceService = interfaceService;
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

        public HttpResponseMessage Post(ItInterfaceExhibitDTO dto)
        {
            return _interfaceService
                .UpdateExposingSystem(dto.ItInterfaceId, dto.ItSystemId)
                .Match(_ => NewObjectCreated(_.ExhibitedBy), FromOperationError);
        }

        [NonAction]
        public override HttpResponseMessage Post(int organizationId, ItInterfaceExhibitDTO dto) => throw new NotSupportedException();

        /// <param name="id">Interface id</param>
        /// <param name="organizationId">Not used</param>
        /// <returns></returns>
        public override HttpResponseMessage Delete(int id, int organizationId)
        {
            Maybe<ItInterface> sourceInterface = _interfaceRepository.GetByKey(id);

            return sourceInterface
                .Match
                (
                    onValue: val => _interfaceService
                        .UpdateExposingSystem(val.Id, null)
                        .Match(_ => Ok(), FromOperationError),
                    onNone: NotFound
                );
        }

        [NonAction]
        public override HttpResponseMessage GetSingle(int id) => throw new NotSupportedException();

        /// <param name="id">Interface id</param>
        /// <param name="organizationId">Not used</param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            const string changeSystemProperty = nameof(ItInterfaceExhibitDTO.ItSystemId);

            if (!obj.TryGetValue(changeSystemProperty, StringComparison.OrdinalIgnoreCase,out var token))
            {
                return BadRequest(changeSystemProperty + " was not provided");
            }

            var systemId = token.Value<int>();

            Maybe<ItInterface> sourceInterface = _interfaceRepository.GetByKey(id);

            return sourceInterface
                .Match
                (
                    onValue: val => _interfaceService
                        .UpdateExposingSystem(val.Id, systemId)
                        .Match(_ => Ok(), FromOperationError),
                    onNone: NotFound
                );
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItInterfaceExhibit, ItInterface>(x => _interfaceRepository.GetByKey(x.Id), base.GetCrudAuthorization());
        }
    }
}
