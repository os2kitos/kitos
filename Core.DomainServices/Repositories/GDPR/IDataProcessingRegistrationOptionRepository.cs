
using System;
using System.Collections.Generic;
using Core.DomainModel.GDPR;
using Core.DomainServices.Options;

namespace Core.DomainServices.Repositories.GDPR
{
    public interface IDataProcessingRegistrationOptionRepository
    {
        //TODO: Review: Why the long name. The type tells the story
        IEnumerable<OptionDescriptor<DataProcessingDataResponsibleOption>> GetAvailableDataResponsibleOptionsWithLocallyUpdatedDescriptions(int organizationId);
        [Obsolete("Let's be thorough here.. all of these should work on option descriptors")]
        IEnumerable<DataProcessingCountryOption> GetAvailableCountryOptions(int organizationId);
        [Obsolete("Replace by extending the registration options (from that the controller may create the 'id maps')")]
        ISet<int> GetIdsOfAvailableCountryOptions(int organizationId);
        [Obsolete("Replace by extending the registration options (from that the controller may create the 'id maps')")]
        ISet<int> GetIdsOfAvailableDataResponsibleOptions(int organizationId);
    }
}
