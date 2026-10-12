using Core.Abstractions.Helpers;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.SupplierAssociatedFields;
using Core.DomainServices.Repositories.Organization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.DomainServices.Suppliers
{
    public class SupplierFieldDomainService : ISupplierFieldDomainService
    {
        private readonly IEnumerable<SupplierAssociatedFieldConfiguration> _defaultFieldConfigurations;
        private readonly IOrganizationRepository _organizationRepository;

        public SupplierFieldDomainService(IOrganizationRepository organizationRepository)
        {
            _defaultFieldConfigurations = new List<SupplierAssociatedFieldConfiguration>
            {
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<DataProcessingRegistration>(x => x.IsOversightCompleted), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightDate), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightRemark), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightReportLink), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightOptionId), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.ContainsAITechnology), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.SystemUsageCriticalityLevel), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.preriskAssessment), ControlState = FieldControlState.Supplier },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightReportLinkName), ControlState = FieldControlState.Shared },
                new SupplierAssociatedFieldConfiguration { FieldKey = ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.riskAssessment), ControlState = FieldControlState.Shared },
            };

            _organizationRepository = organizationRepository;
        }

        public bool ContainsOnlySupplierControlledAndSharedFields(IEnumerable<string> properties, Guid organizationUuid)
        {
            return GetSupplierAssociatedFieldConfigurations(organizationUuid)
                .Match(
                    (organizationalConfigurations) =>
                       {
                        foreach (var property in properties)
                        {
                            var organizationalField = organizationalConfigurations.FirstOrDefault(x => x.FieldKey == property);
                            if (organizationalField != null && !(organizationalField.HasSupplierControlState || organizationalField.HasSharedControlState))
                                return false;

                            if (organizationalField == null && !HasSupplierOrSharedAccessInDefaultConfigurations(property))
                                return false;
                        }
                        return true;
                    },
                    () => properties.All(HasSupplierOrSharedAccessInDefaultConfigurations)
            );
        }

        public bool ContainsAnySupplierControlledFields(IEnumerable<string> properties, Guid organizationUuid)
        {
            return GetSupplierAssociatedFieldConfigurations(organizationUuid).Match(
                (organizationalConfigurations) =>
                {
                    foreach (var property in properties)
                    {
                        var organizationalField = organizationalConfigurations.FirstOrDefault(x => x.FieldKey == property);
                        if (organizationalField != null && organizationalField.HasSupplierControlState)
                            return true;

                        if (organizationalField == null && IsSupplierControlledInDefaultConfigurations(property))
                            return true;
                    }

                    return false;
                },
                () => properties.Any(IsSupplierControlledInDefaultConfigurations)
            );     
        }

        public bool IsSupplierControlled(string key, Guid organizationUuid)
        {
            return GetSupplierAssociatedFieldConfigurations(organizationUuid).Match(
                (organizationalConfigurations) =>
                {
                    var organizationalField = organizationalConfigurations.FirstOrDefault(x => x.FieldKey == key);

                    return organizationalField == null
                        ? IsSupplierControlledInDefaultConfigurations(key)
                        : organizationalField.HasSupplierControlState;
                },
                () => IsSupplierControlledInDefaultConfigurations(key)
            );
        }

        public Maybe<IEnumerable<SupplierAssociatedFieldConfiguration>> GetSupplierAssociatedFieldConfigurations(Guid organizationUuid)
        {
            var organizationMaybe = _organizationRepository.GetByUuid(organizationUuid);
            if (organizationMaybe.IsNone) return Maybe<IEnumerable<SupplierAssociatedFieldConfiguration>>.None;
            var supplierAssociatedFieldConfigurations = organizationMaybe.Value.SupplierAssociatedFieldConfigurations;

            return supplierAssociatedFieldConfigurations.Any()
                ? Maybe<IEnumerable<SupplierAssociatedFieldConfiguration>>.Some(supplierAssociatedFieldConfigurations.AsEnumerable())
                : Maybe<IEnumerable<SupplierAssociatedFieldConfiguration>>.None;
        }

        private SupplierAssociatedFieldConfiguration? TryGetFromDefaultConfiguration(string key)
        {
            return _defaultFieldConfigurations.FirstOrDefault(x => x.FieldKey == key);
        }

        private bool HasSupplierOrSharedAccessInDefaultConfigurations(string key)
        {
            var configuration = TryGetFromDefaultConfiguration(key);
            if (configuration == null) return false;
            return configuration.HasSupplierControlState || configuration.HasSharedControlState;
        }

        private bool IsSupplierControlledInDefaultConfigurations(string key)
        {
            var configuration = TryGetFromDefaultConfiguration(key);
            if (configuration == null) return false;
            return configuration.HasSupplierControlState;
        }
    }
}
