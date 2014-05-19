using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItProject;

namespace UI.MVC4.Models
{
    public class ItProjectDTO
    {
        public int Id { get; set; }
        public string ObjectOwnerName { get; set; }
        public string ItProjectId { get; set; }
        public string Background { get; set; }
        public bool IsTransversal { get; set; }
        public bool IsStrategy { get; set; }
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
        public bool IsStakeholdersVisible { get; set; }
        public bool IsCommunicationVisible { get; set; }
        public bool IsHandoverVisible { get; set; }

        public int? AssociatedProgramId { get; set; }
        public string AssociatedProgramName { get; set; }

        public IEnumerable<int> AssociatedProjectIds { get; set; }

        public int ItProjectTypeId { get; set; }
        public string ItProjectTypeName { get; set; }
        public int ItProjectCategoryId { get; set; }
        public string ItProjectCategoryName { get; set; }
        public int OrganizationId { get; set; }
        public IEnumerable<OrgUnitDTO> UsedByOrgUnits { get; set; }
        public IEnumerable<EconomyYearDTO> EconomyYears { get; set; } 
        public IEnumerable<ItSystemDTO> ItSystems { get; set; }
        public IEnumerable<TaskRefDTO> TaskRefs { get; set; }
        public IEnumerable<RiskDTO> Risks { get; set; }


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
        public ActivityDTO Phase1 { get; set; }
        public ActivityDTO Phase2 { get; set; }
        public ActivityDTO Phase3 { get; set; }
        public ActivityDTO Phase4 { get; set; }
        public ActivityDTO Phase5 { get; set; } 

        /// <summary>
        /// The id of current selected phase
        /// </summary>
        public int? CurrentPhaseId { get; set; }

        /// <summary>
        /// The tasks for "milestones and tasks" table. 
        /// </summary>
        public IEnumerable<ActivityDTO> TaskActivities { get; set; }
        public virtual ICollection<StateDTO> MilestoneStates { get; set; } 

        #endregion

        public int? ResponsibleOrgUnitId { get; set; }
        public OrgUnitDTO ResponsibleOrgUnit { get; set; }

        public int? ParentItProjectId { get; set; }
        public ItProjectDTO ParentItProject { get; set; }



        public virtual GoalStatusDTO GoalStatus { get; set; }

        public int? Roi
        {
            get
            {
                var firstYear = EconomyYears.FirstOrDefault(dto => dto.TotalBudget >= 0);
                return firstYear != null ? (int?) firstYear.YearNumber : null;
            }
        }

        public int Bc
        {
            get { return EconomyYears.Sum(year => year.TotalBudget); }
        }

        public double AverageRisk
        {
            get { return Risks.Any() ? Risks.Average(risk => risk.Consequence * risk.Probability) : default(double); }
        }
    }
}