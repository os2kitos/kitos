using System;

namespace Presentation.Web.Models.API.V1.GDPR
{
    public class DataProcessingRegistrationOversightDateDTO
    {
        public int Id { get; set; }
        public DateTime OversightDate { get; set; }
        public string OversightRemark { get; set; }
    }
}