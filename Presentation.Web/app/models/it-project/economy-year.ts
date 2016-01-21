module Kitos.Models.ItProject {
	/** Economy for an it project in a specific year. */
    export interface IEconomyYear extends IEntity {
        YearNumber: number;
        /** Gets or sets the associated it project identifier. */
        ItProjectId: number;
        /** Gets or sets the associated it project. */
        ItProject: IItProject;
        ConsultantBudget: number;
        ConsultantRea: number;
        EducationBudget: number;
        EducationRea: number;
        OtherBusinessExpensesBudget: number;
        OtherBusinessExpensesRea: number;
        IncreasedBusinessExpensesBudget: number;
        IncreasedBusinessExpensesRea: number;
        HardwareBudget: number;
        HardwareRea: number;
        SoftwareBudget: number;
        SoftwareRea: number;
        OtherItExpensesBudget: number;
        OtherItExpensesRea: number;
        IncreasedItExpensesBudget: number;
        IncreasedItExpensesRea: number;
        SalaryBudget: number;
        SalaryRea: number;
        OtherBusinessSavingsBudget: number;
        OtherBusinessSavingsRea: number;
        LicenseSavingsBudget: number;
        LicenseSavingsRea: number;
        SystemMaintenanceSavingsBudget: number;
        SystemMaintenanceSavingsRea: number;
        OtherItSavingsBudget: number;
        OtherItSavingsRea: number;
    }
}
