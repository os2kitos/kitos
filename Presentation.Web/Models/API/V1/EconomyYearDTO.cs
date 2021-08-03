namespace Presentation.Web.Models.API.V1
{
    public class EconomyYearDTO
    {
        public int Id { get; set; }

        public int YearNumber { get; set; }

        public int ItProjectId { get; set; }
        public virtual ItProjectDTO ItProject { get; set; }

        //Business expenses
        public int ConsultantBudget { get; set; }
        public int ConsultantRea { get; set; }

        public int EducationBudget { get; set; }
        public int EducationRea { get; set; }

        public int OtherBusinessExpensesBudget { get; set; }
        public int OtherBusinessExpensesRea { get; set; }

        public int IncreasedBusinessExpensesBudget { get; set; }
        public int IncreasedBusinessExpensesRea { get; set; }

        public int BusinessExpensesTotal
        {
            get { return ConsultantBudget + EducationBudget + OtherBusinessExpensesBudget + IncreasedBusinessExpensesBudget; }
        }

        //IT expenses
        public int HardwareBudget { get; set; }
        public int HardwareRea { get; set; }

        public int SoftwareBudget { get; set; }
        public int SoftwareRea { get; set; }

        public int OtherItExpensesBudget { get; set; }
        public int OtherItExpensesRea { get; set; }

        public int IncreasedItExpensesBudget { get; set; }
        public int IncreasedItExpensesRea { get; set; }

        public int ItExpensesTotal
        {
            get { return HardwareBudget + SoftwareBudget + OtherItExpensesBudget + IncreasedItExpensesBudget; }
        }

        //Business savings
        public int SalaryBudget { get; set; }
        public int SalaryRea { get; set; }

        public int OtherBusinessSavingsBudget { get; set; }
        public int OtherBusinessSavingsRea { get; set; }

        public int BusinessSavingsTotal
        {
            get { return SalaryBudget + OtherBusinessSavingsBudget; }
        }

        //IT savings
        public int LicenseSavingsBudget { get; set; }
        public int LicenseSavingsRea { get; set; }

        public int SystemMaintenanceSavingsBudget { get; set; }
        public int SystemMaintenanceSavingsRea { get; set; }

        public int OtherItSavingsBudget { get; set; }
        public int OtherItSavingsRea { get; set; }


        public int ItSavingsTotal
        {
            get { return LicenseSavingsBudget + SystemMaintenanceSavingsBudget + OtherItSavingsBudget; }
        }

        public int TotalBudget
        {
            get { return BusinessSavingsTotal + ItSavingsTotal - BusinessExpensesTotal - ItExpensesTotal; }
        }
    }
}
