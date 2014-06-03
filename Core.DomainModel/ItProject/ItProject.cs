using System;
using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel.ItProject
{
    public class ItProject : HasRightsEntity<ItProject, ItProjectRight, ItProjectRole>, IHasAccessModifier
    {
        public ItProject()
        {
            this.Communications = new List<Communication>();
            //this.Economies = new List<Economy>();
            //this.ExtReferences = new List<ExtReference>();
            this.TaskRefs = new List<TaskRef>();
            //this.Resources = new List<Resource>();
            this.Risks = new List<Risk>();
            this.Stakeholders = new List<Stakeholder>();
            this.ItSystemUsages = new List<ItSystemUsage>();
            this.UsedByOrgUnits = new List<OrganizationUnit>();
            this.ItSystemUsages = new List<ItSystemUsage>();
            this.EconomyYears = new List<EconomyYear>();
            this.JointMunicipalProjects = new List<ItProject>();
            this.CommonPublicProjects = new List<ItProject>();
            this.AssociatedProjects = new List<ItProject>();
            this.ChildItProjects = new List<ItProject>();
            this.Priority = ItProjectPriority.None; // default value if not set
            this.PriorityPf = ItProjectPriority.None; // default value if not set
            this.AccessModifier = AccessModifier.Normal;
        }
        
        public string ItProjectId { get; set; }
        public string Background { get; set; }
        public bool IsTransversal { get; set; }
        public string Note { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public ItProjectPriority Priority { get; set; }
        public bool IsPriorityLocked { get; set; }
        public ItProjectPriority PriorityPf { get; set; }
        public bool IsArchived { get; set; }

        public bool IsStatusGoalVisible { get; set; }
        public bool IsEconomyVisible { get; set; }
        public bool IsStakeholderVisible { get; set; }
        public bool IsCommunicationVisible { get; set; }
        public bool IsHandoverVisible { get; set; }
        
        public int? AssociatedProgramId { get; set; }
        public virtual ItProject AssociatedProgram { get; set; }
        public virtual ICollection<ItProject> AssociatedProjects { get; set; }

        public int ItProjectTypeId { get; set; }
        public virtual ItProjectType ItProjectType { get; set; }

        public int ItProjectCategoryId { get; set; }
        public virtual ItProjectCategory ItProjectCategory { get; set; }

        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        public virtual Handover Handover { get; set; }
        public virtual ICollection<Communication> Communications { get; set; }
        //public virtual ICollection<ExtReference> ExtReferences { get; set; } // TODO
        public virtual ICollection<TaskRef> TaskRefs { get; set; }
        public virtual ICollection<Risk> Risks { get; set; }
        public virtual ICollection<Stakeholder> Stakeholders { get; set; }

        /// <summary>
        /// Determines if this project is an IT digitization strategy
        /// </summary>
        public bool IsStrategy { get; set; }

        public int? JointMunicipalProjectId { get; set; }
        public virtual ItProject JointMunicipalProject { get; set; }
        public virtual ICollection<ItProject> JointMunicipalProjects { get; set; }

        public int? CommonPublicProjectId { get; set; }
        public virtual ItProject CommonPublicProject { get; set; }
        public virtual ICollection<ItProject> CommonPublicProjects { get; set; }

        public int? ResponsibleOrgUnitId { get; set; }
        /// <summary>
        /// Organization Unit responsible for this project
        /// </summary>
        public OrganizationUnit ResponsibleOrgUnit { get; set; }

        /// <summary>
        /// These Organization Units are using this project
        /// </summary>
        public virtual ICollection<OrganizationUnit> UsedByOrgUnits { get; set; }
        public virtual ICollection<ItSystemUsage> ItSystemUsages { get; set; }
        public virtual ICollection<EconomyYear> EconomyYears { get; set; }

        #region Status project tab

        /// <summary>
        /// Traffic-light dropdown for overall status
        /// </summary>
        public int StatusProject { get; set; }
        /// <summary>
        /// Date-for-status-update field
        /// </summary>
        public DateTime StatusDate { get; set; }

        /// <summary>
        /// Notes on collected status on project    
        /// </summary>
        public string StatusNote { get; set; }

        // The phases of the project
        public virtual Activity Phase1 { get; set; }
        public virtual Activity Phase2 { get; set; } 
        public virtual Activity Phase3 { get; set; } 
        public virtual Activity Phase4 { get; set; } 
        public virtual Activity Phase5 { get; set; } 
        
        /// <summary>
        /// The id of current selected phase
        /// </summary>
        public int? CurrentPhaseId { get; set; }

        /// <summary>
        /// The tasks for "milestones and tasks" table. 
        /// </summary>
        public virtual ICollection<Activity> TaskActivities { get; set; }
        public virtual ICollection<State> MilestoneStates { get; set; } 

        #endregion

        public virtual int? ParentItProjectId { get; set; }
        public virtual ItProject ParentItProject { get; set; }
        public virtual ICollection<ItProject> ChildItProjects { get; set; }

        public virtual GoalStatus GoalStatus { get; set; }
    }
}
