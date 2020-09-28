using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Result;
using Core.DomainServices.Extensions;
using Core.DomainServices.Options;
using Infrastructure.Services.Types;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationDataResponsibleAssigmentService : IDataProcessingRegistrationDataResponsibleAssignmentService
    {
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> _localDataResponsibleOptionsService;
        private readonly IGenericRepository<LocalDataProcessingDataResponsibleOption> _localDataResponsibleRepository;

        public DataProcessingRegistrationDataResponsibleAssigmentService(
            IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> localDataResponsibleOptionsService,
            IGenericRepository<LocalDataProcessingDataResponsibleOption> localDataResponsibleRepository)
        {
            _localDataResponsibleOptionsService = localDataResponsibleOptionsService;
            _localDataResponsibleRepository = localDataResponsibleRepository;
        }

        public IEnumerable<DataProcessingDataResponsibleOption> GetApplicableDataResponsibleOptionsWithLocalDescriptionOverrides(DataProcessingRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            var availableOptions = _localDataResponsibleOptionsService.GetAvailableOptions(registration.OrganizationId);
            var localDescriptionOverrides = GetLocalDescriptionOverrides(registration.OrganizationId);
            return availableOptions.Select(option => OverrideWithLocalDescription(option, localDescriptionOverrides));
        }

        public Result<DataProcessingRegistration, OperationError> UpdateDataResponsible(DataProcessingRegistration registration, int? dataResponsibleOptionId)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            DataProcessingDataResponsibleOption dataResponsibleOption = null;
            if (dataResponsibleOptionId.HasValue)
            {
                dataResponsibleOption = _localDataResponsibleOptionsService.GetAvailableOption(registration.OrganizationId, dataResponsibleOptionId.Value).Value;
            }            

            registration.DataResponsible = dataResponsibleOption;
            return registration;
        }

        private Dictionary<int, Maybe<string>> GetLocalDescriptionOverrides(int organizationId)
        {
            var localDescriptionOverrides = _localDataResponsibleRepository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .ToDictionary(localDataProcessingRegistrationRole => localDataProcessingRegistrationRole.OptionId,
                    localDataProcessingRegistrationRole => string.IsNullOrWhiteSpace(localDataProcessingRegistrationRole.Description) ? Maybe<string>.None : localDataProcessingRegistrationRole.Description);
            return localDescriptionOverrides;
        }

        private static DataProcessingDataResponsibleOption OverrideWithLocalDescription(DataProcessingDataResponsibleOption option, IReadOnlyDictionary<int, Maybe<string>> localDescriptionOverrides)
        {
            var originalValue = option.Description == null ? "" : option.Description;
            option.Description = localDescriptionOverrides.ContainsKey(option.Id)
                    ? localDescriptionOverrides[option.Id].GetValueOrFallback(originalValue)
                    : option.Description;
            return option;
        }
    }
}
