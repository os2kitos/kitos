using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Core.DomainServices.Extensions;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.Organization;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationDataProcessorAssignmentService : IDataProcessingRegistrationDataProcessorAssignmentService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingCountryOption> _countryOptionsService;
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption> _basisForTransferOptionsService;

        public DataProcessingRegistrationDataProcessorAssignmentService(
            IOrganizationRepository organizationRepository,
            IOptionsService<DataProcessingRegistration, DataProcessingCountryOption> countryOptionsService,
            IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption> basisForTransferOptionsService)
        {
            _organizationRepository = organizationRepository;
            _countryOptionsService = countryOptionsService;
            _basisForTransferOptionsService = basisForTransferOptionsService;
        }

        public IQueryable<Organization> GetApplicableDataProcessors(DataProcessingRegistration registration)
        {
            return GetApplicable(registration, _ => _.DataProcessors);
        }

        public Result<Organization, OperationError> AssignDataProcessor(DataProcessingRegistration registration, int organizationId)
        {
            return ExecuteDataProcessorChange<Organization>(registration, organizationId, _ => _.AssignDataProcessor);
        }

        public Result<Organization, OperationError> RemoveDataProcessor(DataProcessingRegistration registration, int organizationId)
        {
            return ExecuteDataProcessorChange<Organization>(registration, organizationId, _ => _.RemoveDataProcessor);
        }

        public IQueryable<Organization> GetApplicableSubDataProcessors(DataProcessingRegistration registration)
        {
            return GetApplicable(registration, _ => _.AssignedSubDataProcessors.Select(x => x.Organization).ToList());
        }

        public Result<SubDataProcessor, OperationError> AssignSubDataProcessor(DataProcessingRegistration registration, int organizationId)
        {
            return ExecuteDataProcessorChange<SubDataProcessor>(registration, organizationId, _ => _.AssignSubDataProcessor);
        }

        public Result<SubDataProcessor, OperationError> RemoveSubDataProcessor(DataProcessingRegistration registration, int organizationId)
        {
            return ExecuteDataProcessorChange<SubDataProcessor>(registration, organizationId, _ => _.RemoveSubDataProcessor);
        }

        public Result<SubDataProcessor, OperationError> UpdateSubDataProcessor(DataProcessingRegistration registration, int organizationId, int? basisForTransferOptionId, YesNoUndecidedOption? transfer, int? insecureCountryOptionId)
        {
            return ExecuteDataProcessorChange<SubDataProcessor>(registration, organizationId, _ =>
            {
                return (organization) =>
                {
                    var subDataProcessorResult = registration.GetSubDataProcessor(organization);
                    if (subDataProcessorResult.IsNone)
                    {
                        return new OperationError($"Sub data processor with id {organizationId} not assigned to dpr with id {registration.Id}", OperationFailure.NotFound);
                    }
                    var subDataProcessor = subDataProcessorResult.Value;
                    var registrationOrganizationId = registration.Organization.Id;

                    //Get the insecureCountry
                    var countryResult = ResolveOption(registrationOrganizationId, insecureCountryOptionId, subDataProcessor.InsecureCountry.FromNullable(), _countryOptionsService);
                    if (countryResult.Failed)
                    {
                        return countryResult.Error;
                    }

                    //Get the basis for transfer
                    var basisForTransferResult = ResolveOption(registrationOrganizationId, basisForTransferOptionId, subDataProcessor.SubDataProcessorBasisForTransfer.FromNullable(), _basisForTransferOptionsService);
                    if (basisForTransferResult.Failed)
                    {
                        return basisForTransferResult.Error;
                    }
                    return registration.UpdateSubDataProcessor(organization, basisForTransferResult.Value, transfer, countryResult.Value);
                };
            });
        }

        private static Result<Maybe<TOption>, OperationError> ResolveOption<TOption>(int organizationId, int? optionId, Maybe<TOption> existingOption, IOptionsService<DataProcessingRegistration, TOption> optionsService)
            where TOption : OptionEntity<DataProcessingRegistration>
        {
            if (!optionId.HasValue)
                return Maybe<TOption>.None;

            Maybe<TOption> option;
            var newOptionId = optionId.Value;
            if (existingOption.Select(o => o.Id == newOptionId).GetValueOrFallback(false))
            {
                //If already assigned we don't check for availability of the option
                option = existingOption;
            }
            else
            {
                var availableOption = optionsService.GetAvailableOption(organizationId, newOptionId);
                if (availableOption.IsNone)
                {
                    {
                        return new OperationError($"{typeof(TOption).Name} option id {newOptionId} does not point to an available option in organization with id {organizationId}", OperationFailure.BadInput);
                    }
                }

                option = availableOption.Value;
            }

            return option;
        }

        private Result<TResult, OperationError> ExecuteDataProcessorChange<TResult>(
            DataProcessingRegistration registration,
            int organizationId,
            Func<DataProcessingRegistration, Func<Organization, Result<TResult, OperationError>>> command)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return
                _organizationRepository
                    .GetById(organizationId)
                    .Select(command(registration))
                    .Match(result => result, () => new OperationError("Invalid organization id", OperationFailure.BadInput));
        }

        private IQueryable<Organization> GetApplicable(DataProcessingRegistration registration, Func<DataProcessingRegistration, ICollection<Organization>> getTargetCollection)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            var assignedProcessors = getTargetCollection(registration).Select(x => x.Id).ToList();
            return _organizationRepository.GetAll().ExceptEntitiesWithIds(assignedProcessors);
        }
    }
}
