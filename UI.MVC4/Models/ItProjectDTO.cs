using System;
using System.Collections.Generic;
using Core.DomainModel;

namespace UI.MVC4.Models
{
    public class ItProjectDTO
    {
        public int Id { get; set; }
        public string ItProjectId { get; set; }
        public string Background { get; set; }
        public bool IsTransversal { get; set; }
        public string Note { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AccessModifier AccessModifier { get; set; }

        public int? AssociatedProgramId { get; set; }
        public string AssociatedProgramName { get; set; }

        public IEnumerable<int> AssociatedProjectIds { get; set; }

        public int ItProjectTypeId { get; set; }
        public int ItProjectCategoryId { get; set; }
        public int OrganizationId { get; set; }
        public IEnumerable<OrgUnitDTO> UsedByOrgUnits { get; set; }
        public IEnumerable<EconomyYearDTO> EconomyYears { get; set; } 
        public IEnumerable<ItSystemDTO> ItSystems { get; set; }
        public IEnumerable<TaskRefDTO> TaskRefs { get; set; }


        #region Status project tab

        /// <summary>
        /// Traffic-light dropdown for overall status
        /// </summary>
        public int StatusProject { get; set; }
        /// <summary>
        /// Date-for-status-update field
        /// </summary>
        public DateTime StatusDate { get; set; }

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


        #endregion
    }
}