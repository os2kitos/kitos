using System;
using System.ComponentModel;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [RoutePrefix("api/v1/itsystemusage")]
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
        [Route("{id}/sensitivityLevel/add/{dataSensitivityLevel}")]
        public HttpResponseMessage AddSensitivityLevel(int id, int dataSensitivityLevel)
        {
            var sensitivityLevel = MapSensitivityLevel(dataSensitivityLevel);
            var result = _usageService.AddSensitiveDataLevel(id, sensitivityLevel);

            return result.Match
            (
                onSuccess: Ok,
                onFailure: FromOperationError
            );
        }

        [HttpPatch]
        [Route("{id}/sensitivityLevel/remove/{dataSensitivityLevel}")]
        public HttpResponseMessage RemoveSensitivityLevel(int id, int dataSensitivityLevel)
        {
            var sensitivityLevel = MapSensitivityLevel(dataSensitivityLevel);
            var result = _usageService.RemoveSensitiveDataLevel(id, sensitivityLevel);

            return result.Match
            (
                onSuccess: Ok,
                onFailure: FromOperationError
            );
        }

        private DataSensitivityLevel MapSensitivityLevel(int input)
        {
            if (Enum.IsDefined(typeof(DataSensitivityLevel), input))
            {
                return (DataSensitivityLevel)input;
            }
            throw new InvalidEnumArgumentException(nameof(input), input, typeof(DataSensitivityLevel));
        }
    }
}