using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.Extensions;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Notification;
using Core.DomainModel.Organization.Strategies;
using Core.DomainModel.Tracking;
using Core.DomainModel.UIConfiguration;


// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel.Organization
{


    /// <summary>
    /// Represents an Organization (such as a municipality, or a company).
    /// Holds local configuration and admin roles, as well as collections of
    /// ItSystems, etc that was created in this organization.
    /// </summary>
    public class Organization : Entity, IHasAccessModifier, IOrganizationModule, IHasName, IIsPartOfOrganization, IHasUuid
    {
        public const int MaxNameLength = 100;

        public Organization()
        {
            ItSystems = new List<ItSystem.ItSystem>();
            Supplier = new List<ItContract.ItContract>();
            ItSystemUsages = new List<ItSystemUsage.ItSystemUsage>();
            ItContracts = new List<ItContract.ItContract>();
            OrgUnits = new List<OrganizationUnit>();
            Rights = new List<OrganizationRight>();
            OrganizationOptions = new List<LocalOptionEntity<Entity>>();
            UserNotifications = new List<UserNotification>();
            LifeCycleTrackingEvents = new List<LifeCycleTrackingEvent>();
            LifeCycleTrackingEventsWhereOrganizationIsRightsHolder = new List<LifeCycleTrackingEvent>();
            Uuid = Guid.NewGuid();
            DataResponsibles = new List<DataResponsible>();
            DataProtectionAdvisors = new List<DataProtectionAdvisor>();
            IsDefaultOrganization = null;
            ItInterfaces = new List<ItInterface>();
            DataProcessorForDataProcessingRegistrations = new List<DataProcessingRegistration>();
            SubDataProcessorForDataProcessingRegistrations = new List<DataProcessingRegistration>();
            BelongingSystems = new List<ItSystem.ItSystem>();
            UIModuleCustomizations = new List<UIModuleCustomization>();
            ArchiveSupplierForItSystems = new List<ItSystemUsage.ItSystemUsage>();
        }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Adress { get; set; }
        public string Email { get; set; }
        public int TypeId { get; set; }
        public virtual OrganizationType Type { get; set; }
        /// <summary>
        /// Cvr number
        /// </summary>
        /// <remarks>
        /// This is a string instead of int because it's much easier to do a partial search on strings
        /// </remarks>
        public string Cvr { get; set; }
        public string ForeignCvr { get; set; }

        public string GetActiveCvr() => Cvr ?? ForeignCvr;

        public AccessModifier AccessModifier { get; set; }
        public Guid Uuid { get; set; }
        public virtual ICollection<OrganizationUnit> OrgUnits { get; set; }

        public virtual ICollection<LocalOptionEntity<Entity>> OrganizationOptions { get; set; }

        //Systems that belongs to this organization (OIO term - think "produced by IRL")
        public virtual ICollection<ItSystem.ItSystem> BelongingSystems { get; set; }
        //KITOS term - which organization was this system created under in KITOS
        public virtual ICollection<ItSystem.ItSystem> ItSystems { get; set; }

        //KITOS term - which organization was this interface created under in KITOS
        public virtual ICollection<ItSystem.ItInterface> ItInterfaces { get; set; }


        public virtual ICollection<UserNotification> UserNotifications { get; set; }

        /// <summary>
        /// Organization is marked as supplier in these contracts
        /// </summary>
        public virtual ICollection<ItContract.ItContract> Supplier { get; set; }

        /// <summary>
        /// ItContracts created inside the organization
        /// </summary>
        public virtual ICollection<ItContract.ItContract> ItContracts { get; set; }

        /// <summary>
        /// Local usages of IT systems within this organization
        /// </summary>
        public virtual ICollection<ItSystemUsage.ItSystemUsage> ItSystemUsages { get; set; }

        /// <summary>
        /// Local configuration of KITOS
        /// </summary>
        public virtual Config Config { get; set; }

        public virtual ICollection<OrganizationRight> Rights { get; set; }

        public virtual ICollection<StsOrganizationIdentity> StsOrganizationIdentities { get; set; }

        public virtual ICollection<DataProcessingRegistration> DataProcessingRegistrations { get; set; }

        public virtual ICollection<DataProcessingRegistrationReadModel> DataProcessingRegistrationReadModels { get; set; }

        public virtual int? ContactPersonId { get; set; }

        public virtual ContactPerson ContactPerson { get; set; }
        public virtual ICollection<DataProcessingRegistration> DataProcessorForDataProcessingRegistrations { get; set; }
        public virtual ICollection<DataProcessingRegistration> SubDataProcessorForDataProcessingRegistrations { get; set; }
        public virtual ICollection<ItSystemUsageOverviewReadModel> ItSystemUsageOverviewReadModels { get; set; }
        public virtual ICollection<LifeCycleTrackingEvent> LifeCycleTrackingEvents { get; set; }
        public virtual ICollection<LifeCycleTrackingEvent> LifeCycleTrackingEventsWhereOrganizationIsRightsHolder { get; set; }
        public virtual ICollection<DataResponsible> DataResponsibles { get; set; }
        public virtual ICollection<DataProtectionAdvisor> DataProtectionAdvisors { get; set; }

        public virtual ICollection<UIModuleCustomization> UIModuleCustomizations { get; set; }
        public virtual ICollection<ItSystemUsage.ItSystemUsage> ArchiveSupplierForItSystems { get; set; }
        public virtual StsOrganizationConnection StsOrganizationConnection { get; set; }


        /// <summary>
        /// Determines if this is the "Default" organization in KITOS
        /// </summary>
        public bool? IsDefaultOrganization { get; set; }

        public virtual ICollection<ItContractOverviewReadModel> ItContractOverviewReadModels { get; set; }

        /// <summary>
        /// Get the level-0 organization unit, which by convention is named represently
        /// </summary>
        /// <returns></returns>
        public OrganizationUnit GetRoot()
        {
            return OrgUnits.FirstOrDefault(u => u.Parent == null);
        }

        public IEnumerable<int> GetOrganizationIds() => new[] { Id };

        public Maybe<OrganizationUnit> GetOrganizationUnit(Guid organizationUnitId)
        {
            return OrgUnits.FirstOrDefault(unit => unit.Uuid == organizationUnitId);
        }

        public Maybe<UIModuleCustomization> GetUiModuleCustomization(string module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module));

            return UIModuleCustomizations
                .SingleOrDefault(config => config.Module == module)
                .FromNullable();
        }

        public Result<UIModuleCustomization, OperationError> ModifyModuleCustomization(string module, IEnumerable<CustomizedUINode> nodes)
        {
            if (string.IsNullOrEmpty(module))
                throw new ArgumentNullException(nameof(module));
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            var uiNodes = nodes.ToList();
            var customizedUiNodes = uiNodes.ToList();

            var moduleCustomization = GetUiModuleCustomization(module).GetValueOrDefault();
            if (moduleCustomization == null)
            {
                moduleCustomization = new UIModuleCustomization { Organization = this, Module = module };
                UIModuleCustomizations.Add(moduleCustomization);
            }

            var nodeUpdateResult = moduleCustomization.UpdateConfigurationNodes(customizedUiNodes);

            if (nodeUpdateResult.HasValue)
                return nodeUpdateResult.Value;

            return moduleCustomization;
        }

        public Result<OrganizationTreeUpdateConsequences, OperationError> ComputeExternalOrganizationHierarchyUpdateConsequences(OrganizationUnitOrigin origin, ExternalOrganizationUnit root, Maybe<int> levelsIncluded)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            IExternalOrganizationalHierarchyUpdateStrategy strategy;
            //Pre-validate
            switch (origin)
            {
                case OrganizationUnitOrigin.STS_Organisation:
                    if (StsOrganizationConnection?.Connected != true)
                    {
                        return new OperationError($"Not connected to {origin:G}. Please connect before performing an update", OperationFailure.Conflict);
                    }
                    strategy = StsOrganizationConnection.GetUpdateStrategy();
                    break;
                case OrganizationUnitOrigin.Kitos:
                    return new OperationError("Kitos is not an external source", OperationFailure.BadInput);
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return strategy.ComputeUpdate(root, levelsIncluded);
        }

        public Maybe<OperationError> ConnectToExternalOrganizationHierarchy(OrganizationUnitOrigin origin, ExternalOrganizationUnit root, Maybe<int> levelsIncluded)
        {
            //TODO: Get the import strategy based on the connection
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }
            //Pre-validate
            switch (origin)
            {
                case OrganizationUnitOrigin.STS_Organisation:
                    if (StsOrganizationConnection?.Connected == true)
                    {
                        return new OperationError($"Already connected to {origin:G}", OperationFailure.Conflict);
                    }
                    break;
                case OrganizationUnitOrigin.Kitos:
                    return new OperationError("Kitos is not an external source", OperationFailure.BadInput);
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return GetRoot()
                .FromNullable()
                .Match
                (
                    currentOrgRoot =>
                    {
                        var childLevelsToInclude = levelsIncluded.Select(levels => levels - 1); //Subtract one since first level is the root
                        return currentOrgRoot.ImportNewExternalOrganizationOrgTree(origin, root.Copy(childLevelsToInclude));
                    },
                    () => new OperationError("Unable to load current root", OperationFailure.UnknownError)
                ).Match
                (error => error,
                    () =>
                    {
                        StsOrganizationConnection ??= new StsOrganizationConnection();
                        StsOrganizationConnection.Connected = true;
                        StsOrganizationConnection.SynchronizationDepth = levelsIncluded.Match(levels => (int?)levels, () => default);
                        return Maybe<OperationError>.None;
                    }
                );
        }

        public Result<DisconnectOrganizationFromOriginResult, OperationError> DisconnectOrganizationFromExternalSource(OrganizationUnitOrigin origin)
        {
            switch (origin)
            {
                case OrganizationUnitOrigin.STS_Organisation:

                    if (StsOrganizationConnection?.Connected != true)
                    {
                        return new OperationError("Not connected", OperationFailure.BadState);
                    }
                    return StsOrganizationConnection.Disconnect();
                case OrganizationUnitOrigin.Kitos:
                    return new OperationError("Kitos is not an external source and cannot be disconnected", OperationFailure.BadInput);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}