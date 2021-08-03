using System;

namespace Presentation.Web.Models.API.V1.GDPR
{
    public class CreateDataProcessingRegistrationOversightDateDTO
    {
        public DateTime OversightDate { get; set; }
        public string OversightRemark { get; set; }
    }
}