using System.Collections.Generic;
using Core.DomainModel.GDPR;

namespace Core.ApplicationServices.Model.GDPR
{
    public class DataProcessingRegistrationOptions
    {
        public IEnumerable<DataProcessingDataResponsibleOption> DataProcessingRegistrationDataResponsibleOptions { get; }
        public IEnumerable<DataProcessingCountryOption> DataProcessingRegistrationCountryOptions { get; }

        public DataProcessingRegistrationOptions(
            IEnumerable<DataProcessingDataResponsibleOption> dataProcessingRegistrationDataResponsibleOptions,
            IEnumerable<DataProcessingCountryOption> dataProcessingRegistrationCountryOptions)
        {
            DataProcessingRegistrationDataResponsibleOptions = dataProcessingRegistrationDataResponsibleOptions;
            DataProcessingRegistrationCountryOptions = dataProcessingRegistrationCountryOptions;
        }
    }
}
