
using System.Collections.Generic;
using Core.DomainModel.GDPR;

namespace Core.DomainServices.Repositories.GDPR
{
    public interface IDataProcessingRegistrationOptionRepository
    {
        IEnumerable<DataProcessingDataResponsibleOption> GetAvailableDataResponsibleOptions(int organizationId);
        IEnumerable<DataProcessingCountryOption> GetAvailableCountryOptions(int organizationId);
    }
}
