

using System.Web.Http;
using Core.ApplicationServices.GlobalOptions;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Controllers.API.V2.Internal.GlobalOptionTypes
{
    public class BaseGlobalRegularOptionTypesInternalV2Controller<TReferenceType, TOptionType> : InternalApiV2Controller
        where TOptionType : OptionEntity<TReferenceType>, new()
    {
        private readonly IGenericGlobalOptionsService<TOptionType, TReferenceType> _globalOptionsService;
        private readonly IGlobalOptionTypeResponseMapper _responseMapper;
        private readonly IGlobalOptionTypeWriteModelMapper _writeModelMapper;

        public BaseGlobalRegularOptionTypesInternalV2Controller(IGenericGlobalOptionsService<TOptionType, TReferenceType> globalOptionsService, IGlobalOptionTypeResponseMapper responseMapper, IGlobalOptionTypeWriteModelMapper writeModelMapper)
        {
            _globalOptionsService = globalOptionsService;
            _responseMapper = responseMapper;
            _writeModelMapper = writeModelMapper;
        }

        protected IHttpActionResult GetAll()
        {
            return _globalOptionsService.GetGlobalOptions()
                .Select(_responseMapper.ToGlobalRegularOptionDTOs<TOptionType, TReferenceType>)
                .Match(Ok, FromOperationError);
        }

        protected IHttpActionResult Create(GlobalOptionCreateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var createParameters = _writeModelMapper.ToGlobalOptionCreateParameters(dto);
            return BadRequest();
            // return _globalOptionsService.CreateGlobalOption(createParameters)
            //todo expose single response mapper method
        }
    }
}