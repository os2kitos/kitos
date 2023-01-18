using System.Linq;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models.API.V2.Internal.Response;

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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(PublicTextsResponseDTO))]
        public IHttpActionResult Get()
        {
            var texts = _textsRepository.AsQueryable().Take(5).ToList();
            var dto = new PublicTextsResponseDTO();
            for (var i = 0; i < texts.Count; i++)
            {
                MapText(texts[i], i, dto);
            }
            return Ok(dto);
        }

        private static void MapText(Text text, int index, PublicTextsResponseDTO dto)
        {
            var textValue = text.Value ?? "";
            switch (index)
            {
                case 0:
                    dto.Introduction = textValue;
                    break;
                case 1:
                    dto.Misc = textValue;
                    break;
                case 2:
                    dto.Guides = textValue;
                    break;
                case 3:
                    dto.StatusMessages = textValue;
                    break;
                case 4:
                    dto.ContactInfo = textValue;
                    break;
            }
        }
    }
}