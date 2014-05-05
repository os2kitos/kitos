using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public class ItProject : IEntity<int>, IHasRights<ItProjectRight>, IHasAccessModifier
    {
        public ItProject()
        {
            //this.Communications = new List<Communication>();
            //this.Economies = new List<Economy>();
            //this.ExtReferences = new List<ExtReference>();
            //this.TaskRefs = new List<TaskRef>();
            //this.Resources = new List<Resource>();
            //this.Risks = new List<Risk>();
            //this.Stakeholders = new List<Stakeholder>();
            this.Rights = new List<ItProjectRight>();
        }

        public int Id { get; set; }
        public string ItProjectId { get; set; }
        public string Background { get; set; }
        public bool IsTransversal { get; set; }
        public string Note { get; set; }
        public string Name { get; set; }
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
        //public virtual ICollection<TaskRef> TaskRefs { get; set; } // TODO      
        //public virtual ICollection<Resource> Resources { get; set; }
        //public virtual ICollection<Risk> Risks { get; set; }
        //public virtual ICollection<Stakeholder> Stakeholders { get; set; }
        public virtual ICollection<ItProjectRight> Rights { get; set; }

        public virtual ICollection<EconomyYear> EconomyYears { get; set; }
    }

    public class EconomyYear : IEntity<int>
    {
        public int Id { get; set; }

        public int ItProjectId { get; set; }
        public virtual ItProject ItProject { get; set; }

        //Business expenses
        public int ConsultantBudget { get; set; }
        public int ConsultantRea { get; set; }

        public int EducationBudget { get; set; }
        public int EducationRea { get; set; }

        public int OtherBusinessExpensesBudget { get; set; }
        public int OtherBusinessExpensesRea { get; set; }

        public int IncreasedBusinessExpensesBudget { get; set; }
        public int IncreasedBusinessExpensesRea { get; set; }


        //IT expenses
        public int HardwareBudget { get; set; }
        public int HardwareRea { get; set; }

        public int SoftwareBudget { get; set; }
        public int SoftwareRea { get; set; }

        public int OtherItExpensesBudget { get; set; }
        public int OtherItExpensesRea { get; set; }

        public int IncreasedItExpensesBudget { get; set; }
        public int IncreasedItExpensesRea { get; set; }

        //Business savings
        public int SalaryBudget { get; set; }
        public int SalaryRea { get; set; }

        public int OtherBusinessSavingsBudget { get; set; }
        public int OtherBusinessSavingsRea { get; set; }

        //IT savings
        public int LicenseSavingsBudget { get; set; }
        public int LicenseSavingsRea { get; set; }

        public int SystemMaintenanceSavingsBudget { get; set; }
        public int SystemMaintenanceSavingsRea { get; set; }

        public int OtherItSavingsBudget { get; set; }
        public int OtherItSavingsRea { get; set; }
    }
}
