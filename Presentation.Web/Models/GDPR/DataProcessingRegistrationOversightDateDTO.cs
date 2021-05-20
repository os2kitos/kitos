using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.GDPR
{
    public class DataProcessingRegistrationOversightDateDTO
    {
        public int Id { get; set; }
        public DateTime OversightDate { get; set; }
        public string OversightRemark { get; set; }
    }
}