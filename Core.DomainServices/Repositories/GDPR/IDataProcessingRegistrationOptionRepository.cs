
using System;
using System.Collections.Generic;
using Core.DomainModel.GDPR;
using Core.DomainServices.Options;

namespace Core.DomainServices.Repositories.GDPR
{
    public interface IDataProcessingRegistrationOptionRepository
    {
        IEnumerable<OptionDescriptor<DataProcessingDataResponsibleOption>> GetAvailableDataResponsibleOptions(int organizationId);
        IEnumerable<OptionDescriptor<DataProcessingCountryOption>> GetAvailableCountryOptions(int organizationId);
        IEnumerable<OptionDescriptor<DataProcessingBasisForTransferOption>> GetAvailableBasisForTransferOptions(int organizationId);
        IEnumerable<OptionDescriptor<DataProcessingOversightOption>> GetAvailableOversightOptions(int organizationId);
    }
}
