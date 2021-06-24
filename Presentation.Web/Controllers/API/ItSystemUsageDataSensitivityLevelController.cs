using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.ItSystemUsage;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    [RoutePrefix("api/v1/itsystemusage")]
    [DenyRightsHoldersAccess]
    public class ItSystemUsageDataSensitivityLevelController : BaseApiController
    {

        private readonly IItSystemUsageService _usageService;

        public ItSystemUsageDataSensitivityLevelController(IItSystemUsageService usageService)
        {
            _usageService = usageService;
        }

        /// <summary>
        /// Opretter en ny systemrelation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dataSensitivityLevel"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}/sensitivityLevel/add")]
        public HttpResponseMessage AddSensitivityLevel(int id, [FromBody]SensitiveDataLevel dataSensitivityLevel)
        {
            var result = _usageService.AddSensitiveDataLevel(id, dataSensitivityLevel);

            return result.Match
            (
                onSuccess: itSystemUsageDataLevel =>  Ok(MapSensitiveDataLevelDTO(itSystemUsageDataLevel)),
                onFailure: FromOperationError
                    
            );
        }

        [HttpPatch]
        [Route("{id}/sensitivityLevel/remove")]
        public HttpResponseMessage RemoveSensitivityLevel(int id, [FromBody]SensitiveDataLevel dataSensitivityLevel)
        {
            var result = _usageService.RemoveSensitiveDataLevel(id, dataSensitivityLevel);

            return result.Match
            (
                onSuccess: itSystemUsageDataLevel => Ok(MapSensitiveDataLevelDTO(itSystemUsageDataLevel)),
                onFailure: FromOperationError
            );
        }

        private static ItSystemUsageSensitiveDataLevelDTO MapSensitiveDataLevelDTO(
            ItSystemUsageSensitiveDataLevel dataLevel)
        {
            return new ItSystemUsageSensitiveDataLevelDTO
            {
                DataSensitivityLevel = dataLevel.SensitivityDataLevel
            };
        }
    }
}