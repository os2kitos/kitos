
using System.Collections.Generic;
using Core.DomainModel.GDPR;
using Core.DomainServices.Options;

namespace Core.DomainServices.Repositories.GDPR
{
    public interface IDataProcessingRegistrationOptionRepository
    {
        IEnumerable<OptionDescriptor<DataProcessingDataResponsibleOption>> GetAvailableDataResponsibleOptionsWithLocallyUpdatedDescriptions(int organizationId);
        IEnumerable<DataProcessingCountryOption> GetAvailableCountryOptions(int organizationId);
        ISet<int> GetIdsOfAvailableCountryOptions(int organizationId);
        ISet<int> GetIdsOfAvailableDataResponsibleOptions(int organizationId);
    }
}
