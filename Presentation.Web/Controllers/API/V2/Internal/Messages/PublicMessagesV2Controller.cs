using System.Linq;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Web.Http;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Controllers.API.V2.Internal.Messages.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response;
using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Controllers.API.V2.Internal.Messages
{
    [RoutePrefix("api/v2/internal/public-messages")]
    public class PublicMessagesV2Controller : InternalApiV2Controller
    {
        private readonly IGenericRepository<Text> _textsRepository;
        private readonly IAuthorizationContext _authorizationContext;

        public PublicMessagesV2Controller(IGenericRepository<Text> textsRepository, IAuthorizationContext authorizationContext)
        {
            _textsRepository = textsRepository;
            _authorizationContext = authorizationContext;
        }

        /// <summary>
        /// Returns public messages from KITOS
        /// </summary>
        [HttpGet]
        [Route]
        [AllowAnonymous]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(PublicMessagesResponseDTO))]
        public IHttpActionResult Get()
        {
            var texts = _textsRepository.AsQueryable().Take(5).ToList();
            var dto = texts.ToTDO();
            return Ok(dto);
        }

        /// <summary>
        /// Returns permissions of the current api client in relation to the public texts resource
        /// </summary>
        [HttpGet]
        [Route("permissions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ResourcePermissionsResponseDTO))]
        public IHttpActionResult GetPermissions()
        {
            //TODO: Move to application service
            var allowModify = _textsRepository
                .AsQueryable()
                .First()
                .Transform(_authorizationContext.AllowModify);
            return Ok(new ResourcePermissionsResponseDTO()
            {
                Delete = false, //resource cannot be deleted
                Read = true, //public data can be read by anyone
                Modify = allowModify
            });
        }
    }
}