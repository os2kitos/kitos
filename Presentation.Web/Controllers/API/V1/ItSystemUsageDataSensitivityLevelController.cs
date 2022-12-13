using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1.ItSystemUsage;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    [RoutePrefix("api/v1/itsystemusage/{id}/sensitivityLevel")]
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
        [Route("add")]
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
        [Route("remove")]
        public HttpResponseMessage RemoveSensitivityLevel(int id, [FromBody]SensitiveDataLevel dataSensitivityLevel)
        {
            var result = _usageService.RemoveSensitiveDataLevel(id, dataSensitivityLevel);

            return result.Match
            (
                onSuccess: itSystemUsageDataLevel => Ok(MapSensitiveDataLevelDTO(itSystemUsageDataLevel)),
                onFailure: FromOperationError
            );
        }

        [HttpPatch]
        [Route("personalData/{personalDataChoice}")]
        public HttpResponseMessage AddPersonalData(int id, GDPRPersonalDataOption personalDataChoice)
        {
            return _usageService.AddPersonalDataOption(id, personalDataChoice)
                .Match(FromOperationError, Ok);
        }

        [HttpDelete]
        [Route("personalData/{personalDataChoice}")]
        public HttpResponseMessage RemovePersonalData(int id, GDPRPersonalDataOption personalDataChoice)
        {
            return _usageService.RemovePersonalDataOption(id, personalDataChoice)
                .Match(FromOperationError, Ok);
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