using System.Collections.Generic;
using Core.DomainModel.GDPR;
using Core.DomainServices.Options;

namespace Core.ApplicationServices.Model.GDPR
{
    public class DataProcessingRegistrationOptions
    {
        public IEnumerable<OptionDescriptor<DataProcessingDataResponsibleOption>> DataProcessingRegistrationDataResponsibleOptions { get; }
        public IEnumerable<OptionDescriptor<DataProcessingCountryOption>> DataProcessingRegistrationCountryOptions { get; }

        public DataProcessingRegistrationOptions(
            IEnumerable<OptionDescriptor<DataProcessingDataResponsibleOption>> dataProcessingRegistrationDataResponsibleOptions,
            IEnumerable<OptionDescriptor<DataProcessingCountryOption>> dataProcessingRegistrationCountryOptions)
        {
            DataProcessingRegistrationDataResponsibleOptions = dataProcessingRegistrationDataResponsibleOptions;
            DataProcessingRegistrationCountryOptions = dataProcessingRegistrationCountryOptions;
        }
    }
}
