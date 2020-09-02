using System;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.GDPR;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Presentation.Web.Models.GDPR;

namespace Presentation.Web.Controllers.API
{
    [RoutePrefix("api/v1/data-processing-agreement")]
    public class DataProcessingAgreementController : BaseApiController
    {
        private readonly IDataProcessingAgreementService _dataProcessingAgreementService;

        public DataProcessingAgreementController(IDataProcessingAgreementService dataProcessingAgreementService)
        {
            _dataProcessingAgreementService = dataProcessingAgreementService;
        }

        [HttpPost]
        [Route]
        public HttpResponseMessage Post([FromBody] CreateDataProcessingAgreementDTO dto)
        {
            if (dto == null)
                return BadRequest("No input parameters provided");

            return _dataProcessingAgreementService
                .Create(dto.OrganizationId, dto.Name)
                .Match(value => Created(ToDTO(value), new Uri(Request.RequestUri + "/" + value.Id)), FromOperationError);
        }

        [HttpGet]
        [Route("{id}")]
        public HttpResponseMessage Get(int id)
        {
            return _dataProcessingAgreementService
                .Get(id)
                .Match(value => Ok(ToDTO(value)), FromOperationError);
        }

        [HttpDelete]
        [Route("{id}")]
        public HttpResponseMessage Delete(int id)
        {
            return _dataProcessingAgreementService
                .Delete(id)
                .Match(value => NoContent(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/name")]
        public HttpResponseMessage ChangeName(int id, [FromBody] SingleValueDTO<string> value)
        {
            ChangedValue<string> changedValue = value.Value;
            return _dataProcessingAgreementService
                .Update(id, changedValue)
                .Match(_ => NoContent(), FromOperationError);
        }

        /// <summary>
        /// Use internally to query whether a new agreement can be created with the suggested parameters
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        [InternalApi]
        [Route("validate/can-create")]
        public HttpResponseMessage CanCreate(int organizationId, string name)
        {
            return _dataProcessingAgreementService
                .ValidateSuggestedNewAgreement(organizationId, name)
                .Select(FromOperationError)
                .GetValueOrFallback(NoContent());
        }

        private static DataProcessingAgreementDTO ToDTO(DataProcessingAgreement value)
        {
            return new DataProcessingAgreementDTO(value.Id, value.Name);
        }
    }
}