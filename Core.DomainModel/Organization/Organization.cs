using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Reports;
using Core.DomainModel.SSO;

// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel.Organization
{
   

    /// <summary>
    /// Represents an Organization (such as a municipality, or a company).
    /// Holds local configuration and admin roles, as well as collections of
    /// ItSystems, ItProjects, etc that was created in this organization.
    /// </summary>
    public class Organization : Entity, IHasAccessModifier, IContextAware, IOrganizationModule, IHasReferences, IHasName
    {
        public Organization()
        {
            ItProjects = new List<ItProject.ItProject>();
            ItSystems = new List<ItSystem.ItSystem>();
            Supplier = new List<ItContract.ItContract>();
            ItSystemUsages = new List<ItSystemUsage.ItSystemUsage>();
            ItContracts = new List<ItContract.ItContract>();
            OrgUnits = new List<OrganizationUnit>();
            Rights = new List<OrganizationRight>();
            DefaultOrganizationForUsers = new List<User>();
            Reports = new List<Report>();
            OrganizationOptions = new List<LocalOptionEntity<Entity>>();
            ExternalReferences = new List<ExternalReference>();
        }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Adress { get; set; }
        public string Email { get; set; }
        public int TypeId { get; set; }
        public virtual ICollection<ExternalReference> ExternalReferences { get; set; }
        public virtual OrganizationType Type { get; set; }
        /// <summary>
        /// Cvr number
        /// </summary>
        /// <remarks>
        /// This is a string instead of int because it's much easier to do a partial search on strings
        /// </remarks>
        public string Cvr { get; set; }
        public string ForeignCvr { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public Guid? Uuid { get; set; }
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

        public virtual ICollection<User> DefaultOrganizationForUsers { get; set; }

        public virtual ICollection<Report> Reports { get; set; }

        public virtual ICollection<SsoOrganizationIdentity> SsoIdentities { get; set; }

        public virtual int? ContactPersonId { get; set; }
        public virtual ContactPerson ContactPerson { get; set; }

        /// <summary>
        /// Get the level-0 organization unit, which by convention is named represently
        /// </summary>
        /// <returns></returns>
        public OrganizationUnit GetRoot()
        {
            return OrgUnits.FirstOrDefault(u => u.Parent == null);
        }

        /// <summary>
        /// Determines whether this instance is within a given organizational context.
        /// </summary>
        /// <param name="organizationId">The organization identifier (context) the user is accessing from.</param>
        /// <returns>
        ///   <c>true</c> if this instance is in the organizational context, otherwise <c>false</c>.
        /// </returns>
        public bool IsInContext(int organizationId)
        {
            return Id == organizationId;
        }
    }
}
