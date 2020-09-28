using System.Collections.Generic;
using Core.DomainModel.GDPR;

namespace Core.ApplicationServices.Model.GDPR
{
    public class DataProcessingRegistrationOptions
    {
        public DataProcessingRegistration Registration { get; set; }
        public IEnumerable<DataProcessingRegistrationRole> DataProcessingRegistrationRoles { get; set; }
        public IEnumerable<DataProcessingDataResponsibleOption> DataProcessingRegistrationDataResponsibleOptions { get; set; }
    }
}
