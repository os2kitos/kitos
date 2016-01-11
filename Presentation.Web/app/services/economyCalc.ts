(function (ng, app) {
    "use strict";

    app.factory("economyCalc", economyCalc);

    function economyCalc() {
        var service = {
            getBusinessExpensesTotal: getBusinessExpensesTotal,
            getItExpensesTotal: getItExpensesTotal,
            getBusinessSavingsTotal: getBusinessSavingsTotal,
            getItSavingsTotal: getItSavingsTotal,
            getTotalBudget: getTotalBudget
        };

        return service;

        ///////////////

        function getBusinessExpensesTotal(economyData) {
            return economyData.ConsultantBudget + economyData.EducationBudget + economyData.OtherBusinessExpensesBudget + economyData.IncreasedBusinessExpensesBudget;
        }

        function getItExpensesTotal(economyData) {
            return economyData.HardwareBudget + economyData.SoftwareBudget + economyData.OtherItExpensesBudget + economyData.IncreasedItExpensesBudget;
        }

        function getBusinessSavingsTotal(economyData) {
            return economyData.SalaryBudget + economyData.OtherBusinessSavingsBudget;
        }

        function getItSavingsTotal(economyData) {
            return economyData.LicenseSavingsBudget + economyData.SystemMaintenanceSavingsBudget + economyData.OtherItSavingsBudget;
        }

        function getTotalBudget(economyData) {
            return getBusinessExpensesTotal(economyData) + getItExpensesTotal(economyData) - getBusinessSavingsTotal(economyData) - getItSavingsTotal(economyData);
        }
    }
})(angular, app);
