using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.Constants;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Notification;
using Core.DomainModel.SSO;
using Core.DomainModel.Tracking;
using Core.DomainModel.UIConfiguration;


// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel.Organization
{


    /// <summary>
    /// Represents an Organization (such as a municipality, or a company).
    /// Holds local configuration and admin roles, as well as collections of
    /// ItSystems, ItProjects, etc that was created in this organization.
    /// </summary>
    public class Organization : Entity, IHasAccessModifier, IOrganizationModule, IHasName, IIsPartOfOrganization, IHasUuid
    {
        public const int MaxNameLength = 100;

        public Organization()
        {
            ItProjects = new List<ItProject.ItProject>();
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

        /// <summary>
        /// ItProjects created inside this organization
        /// </summary>
        public virtual ICollection<ItProject.ItProject> ItProjects { get; set; }

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

        public virtual ICollection<SsoOrganizationIdentity> SsoIdentities { get; set; }

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

        /// <summary>
        /// Determines if this is the "Default" organization in KITOS
        /// </summary>
        public bool? IsDefaultOrganization { get; set; }

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
            var uiNodes = nodes.ToList();
            var customizedUiNodes = uiNodes.ToList();

            var keysValidity = CheckKeysValidity(uiNodes);
            if(keysValidity.HasValue)
                return new OperationError(keysValidity.Value, OperationFailure.BadInput);

            var moduleCustomization = GetUiModuleCustomization(module).GetValueOrDefault();
            if (moduleCustomization == null)
            {
                moduleCustomization = new UIModuleCustomization { OrganizationId = Id, Module = module };
                UIModuleCustomizations.Add(moduleCustomization);
            }

            moduleCustomization.UpdateConfigurationNodes(customizedUiNodes);
            
            return moduleCustomization;
        }

        private static Maybe<string> CheckKeysValidity(IEnumerable<CustomizedUINode> configurations)
        {
            var customizedUiNodes = configurations.ToList();
            var searchExpression = new Regex(UIModuleConfigurationConstants.ConfigurationKeyRegex);
            
            //check if every key matches the Regex expression
            var incorrectKeys = customizedUiNodes.Where(x => searchExpression.Matches(x.Key).Count < 1).ToList();
            if (incorrectKeys.Count > 0)
                return $" One or more keys are incorrect: {string.Join(", ", incorrectKeys)}";
            
            //check if every key is unique
            var groupedKeys = customizedUiNodes.GroupBy(x => x.Key);
            var duplicateKeys = (from keyGroup in groupedKeys where keyGroup.Count() > 1 select keyGroup.Key).ToList();

            return duplicateKeys.Count > 0 
                ? $"One or more keys are duplicate: {string.Join(",", incorrectKeys)}" 
                : Maybe<string>.None;
        }
    }
}
