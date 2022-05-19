using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRemovalConflicts
    {
        public IReadOnlyList<ItSystem> SystemsWithUsagesOutsideTheOrganization { get; }
        public IReadOnlyList<ItInterface> InterfacesExposedOnSystemsOutsideTheOrganization { get; }
        public IReadOnlyList<ItSystem> SystemsExposingInterfacesDefinedInOtherOrganizations { get; }
        public IReadOnlyList<ItSystem> SystemsSetAsParentSystemToSystemsInOtherOrganizations { get; }
        public IReadOnlyList<DataProcessingRegistration> DprInOtherOrganizationsWhereOrgIsDataProcessor { get; }
        public IReadOnlyList<DataProcessingRegistration> DprInOtherOrganizationsWhereOrgIsSubDataProcessor { get; }
        public IReadOnlyList<ItContract> ContractsInOtherOrganizationsWhereOrgIsSupplier { get; }
        public IReadOnlyList<ItSystem> SystemsInOtherOrganizationsWhereOrgIsRightsHolder { get; }

        public OrganizationRemovalConflicts(
            IReadOnlyList<ItSystem> systemsWithUsagesOutsideTheOrganization,
            IReadOnlyList<ItInterface> interfacesExposedOnSystemsOutsideTheOrganization,
            IReadOnlyList<ItSystem> systemsExposingInterfacesDefinedInOtherOrganizations,
            IReadOnlyList<ItSystem> systemsSetAsParentSystemToSystemsInOtherOrganizations,
            IReadOnlyList<DataProcessingRegistration> dprInOtherOrganizationsWhereOrgIsDataProcessor,
            IReadOnlyList<DataProcessingRegistration> dprInOtherOrganizationsWhereOrgIsSubDataProcessor,
            IReadOnlyList<ItContract> contractsInOtherOrganizationsWhereOrgIsSupplier,
            IReadOnlyList<ItSystem> systemsInOtherOrganizationsWhereOrgIsRightsHolder)
        {
            SystemsWithUsagesOutsideTheOrganization = systemsWithUsagesOutsideTheOrganization;
            InterfacesExposedOnSystemsOutsideTheOrganization = interfacesExposedOnSystemsOutsideTheOrganization;
            SystemsExposingInterfacesDefinedInOtherOrganizations = systemsExposingInterfacesDefinedInOtherOrganizations;
            SystemsSetAsParentSystemToSystemsInOtherOrganizations = systemsSetAsParentSystemToSystemsInOtherOrganizations;
            DprInOtherOrganizationsWhereOrgIsDataProcessor = dprInOtherOrganizationsWhereOrgIsDataProcessor;
            DprInOtherOrganizationsWhereOrgIsSubDataProcessor = dprInOtherOrganizationsWhereOrgIsSubDataProcessor;
            ContractsInOtherOrganizationsWhereOrgIsSupplier = contractsInOtherOrganizationsWhereOrgIsSupplier;
            SystemsInOtherOrganizationsWhereOrgIsRightsHolder = systemsInOtherOrganizationsWhereOrgIsRightsHolder;
        }


        public bool Any => SystemsWithUsagesOutsideTheOrganization.Any() ||
                           InterfacesExposedOnSystemsOutsideTheOrganization.Any() ||
                           SystemsExposingInterfacesDefinedInOtherOrganizations.Any() ||
                           SystemsSetAsParentSystemToSystemsInOtherOrganizations.Any() ||
                           DprInOtherOrganizationsWhereOrgIsDataProcessor.Any() ||
                           DprInOtherOrganizationsWhereOrgIsSubDataProcessor.Any() ||
                           ContractsInOtherOrganizationsWhereOrgIsSupplier.Any() ||
                           SystemsInOtherOrganizationsWhereOrgIsRightsHolder.Any();
    }
}
