using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItProject;

namespace Presentation.Web.Models.API.V1
{
    public class ItProjectDTO
    {
        public int Id { get; set; }
        public string ObjectOwnerName { get; set; }
        public string ObjectOwnerLastName { get; set; }

        public string ObjectOwnerFullName
        {
            get { return ObjectOwnerName + " " + ObjectOwnerLastName; }
        }
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
        public string Esdh { get; set; }
        public string Cmdb { get; set; }
        public string Folder { get; set; }
        public int? CommonPublicProjectId { get; set; }
        public int? JointMunicipalProjectId { get; set; }

        public bool IsStatusGoalVisible { get; set; }
        public bool IsStrategyVisible { get; set; }
        public bool IsRiskVisible { get; set; }
        public bool IsHierarchyVisible { get; set; }
        public bool IsEconomyVisible { get; set; }
        public bool IsStakeholderVisible { get; set; }
        public bool IsCommunicationVisible { get; set; }
        public bool IsHandoverVisible { get; set; }

        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public IEnumerable<int> ChildrenIds { get; set; }

        public int? ItProjectTypeId { get; set; }
        public string ItProjectTypeName { get; set; }
        public int OrganizationId { get; set; }
        public IEnumerable<EconomyYearDTO> EconomyYears { get; set; }
        public IEnumerable<TaskRefDTO> TaskRefs { get; set; }
        public IEnumerable<RiskDTO> Risks { get; set; }
        public IEnumerable<StakeholderDTO> Stakeholders { get; set; }


        #region Status project tab

        /// <summary>
        /// Traffic-light dropdown for overall status
        /// </summary>
        public TrafficLight StatusProject { get; set; }
        /// <summary>
        /// Date-for-status-update field
        /// </summary>
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// Notes on collected status on project
        /// </summary>
        public string StatusNote { get; set; }

        // The phases of the project
        public ItProjectPhaseDTO Phase1 { get; set; }
        public ItProjectPhaseDTO Phase2 { get; set; }
        public ItProjectPhaseDTO Phase3 { get; set; }
        public ItProjectPhaseDTO Phase4 { get; set; }
        public ItProjectPhaseDTO Phase5 { get; set; }

        /// <summary>
        /// The id of current selected phase
        /// </summary>
        public int CurrentPhase { get; set; }

        #endregion

        public int? OriginalId { get; set; }
        public ItProjectDTO Original { get; set; }
        public string OriginalItProjectId { get; set; }

        public DateTime LastChanged { get; set; }
        public int LastChangedByUserId { get; set; }

        public virtual GoalStatusDTO GoalStatus { get; set; }

        public ICollection<ExternalReferenceDTO> ExternalReferences { get; set; }

        public int? ReferenceId { get; set; }
        public ExternalReferenceDTO Reference;

        public int? Roi
        {
            get
            {
                if (EconomyYears == null) return null;

                var firstYear = EconomyYears.FirstOrDefault(dto => dto.TotalBudget >= 0);
                return firstYear != null ? (int?) firstYear.YearNumber : null;
            }
        }

        public int Bc
        {
            get
            {
                if (EconomyYears == null) return 0;

                return EconomyYears.Sum(year => year.TotalBudget);
            }
        }

        public double AverageRisk
        {
            get
            {
                if (Risks == null) return default(double);

                return Risks.Any() ? Risks.Average(risk => risk.Consequence * risk.Probability) : default(double);
            }
        }

        public Guid Uuid { get; set; }
    }
}
