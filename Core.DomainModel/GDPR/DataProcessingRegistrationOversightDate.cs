using System;
using System.Collections;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingRegistrationOversightDate
    {
        public int Id { get; set; }

        public DateTime OversightDate { get; set; }
        public string OversightRemark { get; set; }

        public int ParentId { get; set; }
        public virtual DataProcessingRegistration Parent { get; set; }
    }
}
