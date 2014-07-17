using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItProject;

namespace UI.MVC4.Models
{
    public class ItProjectOverviewDTO
    {
        public bool IsArchived { get; set; }
        public bool IsTransversal { get; set; }
        public bool IsStrategy { get; set; }

        //1st column
        public int Id { get; set; }
        public string Name { get; set; }

        //2nd column
        public string ResponsibleOrgUnitName { get; set; }
        
        //3rd column
        public IEnumerable<RightOutputDTO> Rights { get; set; }

        //4th column
        public string ItProjectId { get; set; }
        public string OriginalItProjectId { get; set; }

        //5th
        public string ItProjectTypeName { get; set; }

        //6th
        // The phases of the project
        public ActivitySimpleDTO Phase1 { get; set; }
        public ActivitySimpleDTO Phase2 { get; set; }
        public ActivitySimpleDTO Phase3 { get; set; }
        public ActivitySimpleDTO Phase4 { get; set; }
        public ActivitySimpleDTO Phase5 { get; set; }
        public int? CurrentPhaseId { get; set; }

        //7th
        public int StatusProject { get; set; }
        public int GoalStatusStatus { get; set; }

        //8th
        public IEnumerable<RiskDTO> Risks { get; set; }
        public IEnumerable<EconomyYearDTO> EconomyYears { get; set; }

        public int? Roi
        {
            get
            {
                if (EconomyYears == null) return null;

                var firstYear = EconomyYears.FirstOrDefault(dto => dto.TotalBudget >= 0);
                return firstYear != null ? (int?)firstYear.YearNumber : null;
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

        //9th
        public ItProjectPriority Priority { get; set; }
        public bool IsPriorityLocked { get; set; }
        public ItProjectPriority PriorityPf { get; set; }
    }
}