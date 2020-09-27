using System;
using System.Collections.Generic;
using Core.DomainModel.GDPR;
using Core.DomainServices.Options;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationDataResponsibleAssigmentService : IDataProcessingRegistrationDataResponsibleAssignmentService
    {
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> _localDataResponsibleOptionsService;

        public DataProcessingRegistrationDataResponsibleAssigmentService(
            IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> localDataResponsibleOptionsService)
        {
            _localDataResponsibleOptionsService = localDataResponsibleOptionsService;
        }

        public IEnumerable<DataProcessingDataResponsibleOption> GetApplicableDataResponsibleOptions(DataProcessingRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            return _localDataResponsibleOptionsService.GetAvailableOptions(registration.OrganizationId);
        }
    }
}
