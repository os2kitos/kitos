using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainServices.Options;

namespace Core.ApplicationServices.Model.GDPR
{
    public class DataProcessingRegistrationOptions
    {
        public IReadOnlyList<OptionDescriptor<DataProcessingDataResponsibleOption>> DataResponsibleOptions { get; }
        public IReadOnlyList<OptionDescriptor<DataProcessingCountryOption>> ThirdCountryOptions { get; }
        public IReadOnlyList<OptionDescriptor<DataProcessingBasisForTransferOption>> BasisForTransferOptions { get; }

        public DataProcessingRegistrationOptions(
            IEnumerable<OptionDescriptor<DataProcessingDataResponsibleOption>> dataResponsibleOptions,
            IEnumerable<OptionDescriptor<DataProcessingCountryOption>> thirdCountryOptions,
            IEnumerable<OptionDescriptor<DataProcessingBasisForTransferOption>> basisForTransferOptions)
        {
            DataResponsibleOptions = dataResponsibleOptions.ToList();
            ThirdCountryOptions = thirdCountryOptions.ToList();
            BasisForTransferOptions = basisForTransferOptions.ToList();
        }
    }
}
