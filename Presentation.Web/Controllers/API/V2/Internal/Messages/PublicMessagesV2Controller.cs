using System.Linq;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models.API.V2.Internal.Response;
using Presentation.Web.Controllers.API.V2.Mapping;

namespace Presentation.Web.Controllers.API.V2.Internal.Messages
{
    [RoutePrefix("api/v2/internal/public-messages")]
    public class PublicMessagesV2Controller : InternalApiV2Controller
    {
        private readonly IGenericRepository<Text> _textsRepository;

        public PublicMessagesV2Controller(IGenericRepository<Text> textsRepository)
        {
            _textsRepository = textsRepository;
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
    }
}