using System;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.System.Write;
using Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems;

[RoutePrefix("api/v2/it-systems/{systemUuid}/dbs")]
public class ItSystemDBSV2Controller : ExternalBaseController
{
    private readonly IItSystemWriteService _writeService;
    private readonly ILegalPropertyWriteModelMapper _writeModelMapper;

    public ItSystemDBSV2Controller(IItSystemWriteService writeService, ILegalPropertyWriteModelMapper writeModelMapper)
    {
        _writeService = writeService;
        _writeModelMapper = writeModelMapper;
    }

    [HttpPatch]
    [Route]
    [SwaggerResponse(HttpStatusCode.NoContent)]
    [SwaggerResponse(HttpStatusCode.BadRequest)]
    [SwaggerResponse(HttpStatusCode.Forbidden)]
    [SwaggerResponse(HttpStatusCode.NotFound)]
    [SwaggerResponse(HttpStatusCode.Unauthorized)]
    public IHttpActionResult PatchDbsProperties([NonEmptyGuid][FromUri] Guid systemUuid, [FromBody] LegalPropertiesUpdateRequestDTO request)
    {
        var parameters = _writeModelMapper.FromPATCH(request);
        return _writeService.LegalPropertiesUpdate(systemUuid, parameters)
            .Match(NoContent, FromOperationError);
    }
}