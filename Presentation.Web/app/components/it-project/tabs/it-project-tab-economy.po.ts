import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

class ItProjectEditTabEconomy implements IPageObject {
    controllerVm: string = "";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/economy");
    }

    // budget row repeater
    rowRepeater = new RepeaterWrapper("row in " + this.controllerVm + "rows");
    budgetLocator = by.css(".year-budget-col input:not([readonly])");
    reaLocator = by.css(".year-rea-col input:not([readonly])");
    totalBudgetLocator = by.css(".budget-sum");
    totalReaLocator = by.css(".rea-sum");
    sumBudgetLocator = by.css(".year-budget-col input[readonly]");
    sumReaLocator = by.css(".year-rea-col input[readonly]");
}

export = ItProjectEditTabEconomy;
