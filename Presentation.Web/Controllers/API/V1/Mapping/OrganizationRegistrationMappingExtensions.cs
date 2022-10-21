using Core.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.Organizations;
using Presentation.Web.Models.API.V1.Organizations;

namespace Presentation.Web.Controllers.API.V1.Mapping
{
    public static class OrganizationRegistrationMappingExtensions
    {
        private static readonly IReadOnlyDictionary<OrganizationRegistrationOption, OrganizationRegistrationType> ApiToDataMap;
        private static readonly IReadOnlyDictionary<OrganizationRegistrationType, OrganizationRegistrationOption> DataToApiMap;

        static OrganizationRegistrationMappingExtensions()
        {
            ApiToDataMap = new Dictionary<OrganizationRegistrationOption, OrganizationRegistrationType>
            {
                { OrganizationRegistrationOption.ContractRegistrations, OrganizationRegistrationType.ContractRegistrations},
                { OrganizationRegistrationOption.InternalPayments, OrganizationRegistrationType.InternalPayments},
                { OrganizationRegistrationOption.ExternalPayments, OrganizationRegistrationType.ExternalPayments},
                { OrganizationRegistrationOption.Roles, OrganizationRegistrationType.Roles},
                { OrganizationRegistrationOption.RelevantSystems, OrganizationRegistrationType.RelevantSystems},
                { OrganizationRegistrationOption.ResponsibleSystems, OrganizationRegistrationType.ResponsibleSystems},
            }.AsReadOnly();

            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static OrganizationRegistrationOption ToOrganizationRegistrationOption(this OrganizationRegistrationType value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static OrganizationRegistrationType ToOrganizationRegistrationType(this OrganizationRegistrationOption value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}