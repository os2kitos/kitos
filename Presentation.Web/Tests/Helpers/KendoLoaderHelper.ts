import WaitTimers = require("../Utility/WaitTimers");

class KendoLoaderHelper {

    private waitUpTo = new WaitTimers();
    private ec = protractor.ExpectedConditions;
    private kendoLoadingMaskOff = this.ec.not(
        this.ec.presenceOf(element(by.className("k-loading-mask"))));

    waitForKendoGridData(columnName: protractor.ElementFinder) {

        return browser.wait(this.kendoLoadingMaskOff, this.waitUpTo.twentySeconds)
            .then(() => this.waitForKendoGrid(columnName));
    }

    waitForKendoGrid(columnName: protractor.ElementFinder) {
        return browser
            .wait(this.ec.visibilityOf(columnName), this.waitUpTo.twentySeconds);
    }
}
export = KendoLoaderHelper