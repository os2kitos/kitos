import WaitTimers = require("../Utility/WaitTimers");

class KendoLoaderHelper {

    private waitUpTo = new WaitTimers();
    private ec = protractor.ExpectedConditions;

    readonly kendoLoadingMask = this.ec.not(
        this.ec.presenceOf(element(by.className("k-loading-mask"))));

    waitForKendoGridData(columnName: protractor.ElementFinder) {

        return this.waitForKendoGrid(columnName)
            .then(() => browser.wait(this.kendoLoadingMask, this.waitUpTo.twentySeconds));
    }

    waitForKendoGrid(columnName: protractor.ElementFinder) {
        return browser
            .wait(this.ec.visibilityOf(columnName), this.waitUpTo.twentySeconds);
    }
}
export = KendoLoaderHelper