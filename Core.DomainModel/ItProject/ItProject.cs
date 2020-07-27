using System;
using System.Collections.Generic;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.Result;

// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel.ItProject
{
    public class ItProject : HasRightsEntity<ItProject, ItProjectRight, ItProjectRole>,IHasReferences, IHasAccessModifier, IHierarchy<ItProject>, IContextAware, IProjectModule, IOwnedByOrganization, IHasName, IEntityWithExternalReferences
    {
        public ItProject()
        {
            Communications = new List<Communication>();
            TaskRefs = new List<TaskRef>();
            Risks = new List<Risk>();
            Stakeholders = new List<Stakeholder>();
            UsedByOrgUnits = new List<ItProjectOrgUnitUsage>();
            ItSystemUsages = new List<ItSystemUsage.ItSystemUsage>();
            EconomyYears = new List<EconomyYear>();
            JointMunicipalProjects = new List<ItProject>();
            CommonPublicProjects = new List<ItProject>();
            Children = new List<ItProject>();
            Clones = new List<ItProject>();
            ItProjectStatuses = new List<ItProjectStatus>();
            Phase1 = new ItProjectPhase();
            Phase2 = new ItProjectPhase();
            Phase3 = new ItProjectPhase();
            Phase4 = new ItProjectPhase();
            Phase5 = new ItProjectPhase();
            
            Priority = ItProjectPriority.None;
            PriorityPf = ItProjectPriority.None;
            AccessModifier = AccessModifier.Local;
            ExternalReferences = new List<ExternalReference>();
        }


        /// <summary>
        ///     Gets or sets a value indicating whether this instance is transversal. (tværgående)
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is transversal; otherwise, <c>false</c>.
        /// </value>
        public bool IsTransversal { get; set; }

        public bool IsStatusGoalVisible { get; set; }
        public bool IsStrategyVisible { get; set; }
        public bool IsRiskVisible { get; set; }
        public bool IsHierarchyVisible { get; set; }
        public bool IsEconomyVisible { get; set; }
        public bool IsStakeholderVisible { get; set; }
        public bool IsCommunicationVisible { get; set; }
        public bool IsHandoverVisible { get; set; }

        public virtual Handover Handover { get; set; }
        public virtual ICollection<Communication> Communications { get; set; }
        public virtual ICollection<TaskRef> TaskRefs { get; set; }
        public virtual ICollection<Risk> Risks { get; set; }
        public virtual ICollection<Stakeholder> Stakeholders { get; set; }

        /// <summary>
        ///     Determines if this project is an IT digitalization strategy
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is an strategy; otherwise, <c>false</c>.
        /// </value>
        public bool IsStrategy { get; set; }

        public int? JointMunicipalProjectId { get; set; }
        public virtual ItProject JointMunicipalProject { get; set; }
        public virtual ICollection<ItProject> JointMunicipalProjects { get; set; }

        public int? CommonPublicProjectId { get; set; }
        public virtual ItProject CommonPublicProject { get; set; }
        public virtual ICollection<ItProject> CommonPublicProjects { get; set; }

        /// <summary>
        ///     Organization Unit responsible for this project.
        /// </summary>
        /// <value>
        ///     The responsible org unit.
        /// </value>
        public virtual ItProjectOrgUnitUsage ResponsibleUsage { get; set; }

        /// <summary>
        ///     Organization units that are using this project.
        /// </summary>
        /// <value>
        ///     Organization units using this project.
        /// </value>
        public virtual ICollection<ItProjectOrgUnitUsage> UsedByOrgUnits { get; set; }

        /// <summary>
        ///     Gets or sets the associated it system usages.
        /// </summary>
        /// <remarks>
        ///     <see cref="ItSystemUsage" /> have a corresponding property linking back.
        /// </remarks>
        /// <value>
        ///     Associated it system usages.
        /// </value>
        public virtual ICollection<ItSystemUsage.ItSystemUsage> ItSystemUsages { get; set; }

        public virtual ICollection<EconomyYear> EconomyYears { get; set; }

        /// <summary>
        ///     Gets or sets the original it project identifier this it project was "cloned" from.
        /// </summary>
        /// <value>
        ///     The original it project identifier.
        /// </value>
        public int? OriginalId { get; set; }

        /// <summary>
        ///     Gets or sets the original it project this it project was "cloned" from.
        /// </summary>
        /// <value>
        ///     The original it project.
        /// </value>
        public virtual ItProject Original { get; set; }

        /// <summary>
        ///     Gets or sets the it projects that were cloned from this it project.
        /// </summary>
        /// <value>
        ///     The clones.
        /// </value>
        public virtual ICollection<ItProject> Clones { get; set; } // TODO this isn't used anymore, we should probably remove it

        public virtual GoalStatus GoalStatus { get; set; }

        /// <summary>
        ///     Determines whether this instance is within a given organizational context.
        /// </summary>
        /// <param name="organizationId">The organization identifier (context) the user is accessing from.</param>
        /// <returns>
        ///     <c>true</c> if this instance is in the organizational context, otherwise <c>false</c>.
        /// </returns>
        public bool IsInContext(int organizationId)
        {
            return OrganizationId == organizationId;
        }

        #region Master

        /// <summary>
        ///     Gets or sets the user defined it project identifier.
        /// </summary>
        /// <remarks>
        ///     This is NOT the primary key.
        /// </remarks>
        /// <value>
        ///     It project identifier.
        /// </value>
        public string ItProjectId { get; set; }

        public string Background { get; set; }
        public string Note { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public bool IsArchived { get; set; }
        public string Esdh { get; set; }
        public string Cmdb { get; set; }
        public string Folder { get; set; }
        public int? ParentId { get; set; }
        public virtual ItProject Parent { get; set; }
        public virtual ICollection<ItProject> Children { get; set; }

        public int? ItProjectTypeId { get; set; }
        public virtual ItProjectType ItProjectType { get; set; }


        /// <summary>
        ///     Gets or sets the organization identifier in which this project was created.
        /// </summary>
        /// <value>
        ///     The organization identifier.
        /// </value>
        public int OrganizationId { get; set; }

        /// <summary>
        ///     Gets or sets the organization in which this project was created.
        /// </summary>
        /// <value>
        ///     The organization.
        /// </value>
        public virtual Organization.Organization Organization { get; set; }


        public virtual ICollection<ExternalReference> ExternalReferences { get; set; }
        public ReferenceRootType GetRootType()
        {
            return ReferenceRootType.Project;
        }

        public Result<ExternalReference, OperationError> AddExternalReference(ExternalReference newReference)
        {
            return new AddReferenceCommand(this).AddExternalReference(newReference);
        }

        public Result<ExternalReference, OperationError> SetMasterReference(ExternalReference newReference)
        {
            Reference = newReference;
            return newReference;
        }

        public int? ReferenceId { get; set; }
        public virtual ExternalReference Reference { get; set; }

        #endregion

        #region Overview

        public ItProjectPriority Priority { get; set; }
        public bool IsPriorityLocked { get; set; }
        public ItProjectPriority PriorityPf { get; set; }

        #endregion

        #region Status project tab

        /// <summary>
        ///     Date-for-status-update field
        /// </summary>
        public DateTime? StatusDate { get; set; }

        /// <summary>
        ///     Notes on collected status on project
        /// </summary>
        public string StatusNote { get; set; }

        // The phases of the project
        public ItProjectPhase Phase1 { get; set; }
        public ItProjectPhase Phase2 { get; set; }
        public ItProjectPhase Phase3 { get; set; }
        public ItProjectPhase Phase4 { get; set; }
        public ItProjectPhase Phase5 { get; set; }

        /// <summary>
        ///     The id of current selected phase
        /// </summary>
        public int CurrentPhase { get; set; }

        /// <summary>
        ///     The "milestones and tasks" table.
        /// </summary>
        public virtual ICollection<ItProjectStatus> ItProjectStatuses { get; set; }
        public virtual ICollection<ItProjectStatusUpdate> ItProjectStatusUpdates { get; set; }

        #endregion
    }
}