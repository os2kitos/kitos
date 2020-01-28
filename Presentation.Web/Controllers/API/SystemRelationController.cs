using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.SystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.SystemRelations;

namespace Presentation.Web.Controllers.API
{

    [InternalApi]
    [System.Web.Mvc.RoutePrefix("api/v1/systemrelations")]
    public class SystemRelationController : BaseApiController
    {
        private readonly IItSystemUsageService _usageService;

        public SystemRelationController(IItSystemUsageService usageService)
        {
            _usageService = usageService;
        }

        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.Route("")]
        public HttpResponseMessage PostRelation([FromBody] CreateSystemRelationDTO relation)
        {
            if (relation == null)
                return BadRequest("Missing relation data");

            var result = _usageService.AddRelation(
                relation.SourceUsageId,
                relation.TargetUsageId,
                relation.InterfaceId,
                relation.Description,
                relation.LinkName,
                relation.LinkUrl,
                relation.FrequencyTypeId,
                relation.ContractId);

            return result.Ok
                ? Created($"system-usages/{relation.SourceUsageId}/usage-relations/{result.Value.Id}")
                : FromOperationError(result.Error);
        }
    }
}