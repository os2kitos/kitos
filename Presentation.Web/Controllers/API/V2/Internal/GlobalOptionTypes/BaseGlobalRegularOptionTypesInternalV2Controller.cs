

using System.Web.Http;
using Core.ApplicationServices.GlobalOptions;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;

namespace Presentation.Web.Controllers.API.V2.Internal.GlobalOptionTypes
{
    public class BaseGlobalRegularOptionTypesInternalV2Controller<TReferenceType, TOptionType> : InternalApiV2Controller
        where TOptionType : OptionEntity<TReferenceType>, new()
    {
        private readonly IGenericGlobalOptionsService<TOptionType, TReferenceType> _globalOptionsService;
        private readonly IGlobalOptionTypeResponseMapper _responseMapper;

        public BaseGlobalRegularOptionTypesInternalV2Controller(IGenericGlobalOptionsService<TOptionType, TReferenceType> globalOptionsService, IGlobalOptionTypeResponseMapper responseMapper)
        {
            _globalOptionsService = globalOptionsService;
            _responseMapper = responseMapper;
        }

        protected IHttpActionResult GetAll()
        {
            return _globalOptionsService.GetGlobalOptions()
                .Select(_responseMapper.ToGlobalRegularOptionDTOs<TOptionType, TReferenceType>)
                .Match(Ok, FromOperationError);
        }
    }
}