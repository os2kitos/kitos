using System;
using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel.ItProject
{
    public class ItProject : IEntity<int>, IHasRights<ItProjectRight>, IHasAccessModifier, IHasOwner
    {
        public ItProject()
        {
            //this.Communications = new List<Communication>();
            //this.Economies = new List<Economy>();
            //this.ExtReferences = new List<ExtReference>();
            this.TaskRefs = new List<TaskRef>();
            //this.Resources = new List<Resource>();
            this.Risks = new List<Risk>();
            //this.Stakeholders = new List<Stakeholder>();
            this.Rights = new List<ItProjectRight>();
            this.ItSystemUsages = new List<ItSystemUsage>();
            this.JointMunicipalProjects = new List<ItProject>();
            this.CommonPublicProjects = new List<ItProject>();
            this.AssociatedProjects = new List<ItProject>();
            this.ChildItProjects = new List<ItProject>();
        }

        public int Id { get; set; }
        public string ItProjectId { get; set; }
        public string Background { get; set; }
        public bool IsTransversal { get; set; }
        public string Note { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AccessModifier AccessModifier { get; set; }

        //public bool IsTermsOfReferenceApproved { get; set; }
        //public int? ItProjectOwnerId { get; set; }
        //public int? ItProjectLeaderId { get; set; }
        //public int? PartItProjectLeaderId { get; set; }
        //public int? ConsultantId { get; set; }

        public int ObjectOwnerId { get; set; }
        public virtual User ObjectOwner { get; set; }

        public int? AssociatedProgramId { get; set; }
        public virtual ItProject AssociatedProgram { get; set; }
        public virtual ICollection<ItProject> AssociatedProjects { get; set; }

        public int ItProjectTypeId { get; set; }
        public virtual ItProjectType ItProjectType { get; set; }

        public int ItProjectCategoryId { get; set; }
        public virtual ItProjectCategory ItProjectCategory { get; set; }

        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        //public virtual ProjectStatus ProjectStatus { get; set; }
        //public virtual GoalStatus GoalStatus { get; set; }
        //public virtual Handover Handover { get; set; }
        //public virtual OrgTab OrgTab { get; set; }
        //public virtual Hierarchy Hierarchy { get; set; }
        //public virtual ICollection<Communication> Communications { get; set; }
        //public virtual ICollection<Economy> Economies { get; set; }
        //public virtual ICollection<ExtReference> ExtReferences { get; set; } // TODO
        public virtual ICollection<TaskRef> TaskRefs { get; set; }
        //public virtual ICollection<Resource> Resources { get; set; }
        public virtual ICollection<Risk> Risks { get; set; }
        //public virtual ICollection<Stakeholder> Stakeholders { get; set; }
        public virtual ICollection<ItProjectRight> Rights { get; set; }

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

        public virtual int? ParentItProjectId { get; set; }
        public virtual ItProject ParentItProject { get; set; }
        public virtual ICollection<ItProject> ChildItProjects { get; set; }
    }
}
