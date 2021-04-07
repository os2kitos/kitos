import WaitTimers = require("../Utility/WaitTimers");

class KendoLoaderHelper {

    private waitUpTo = new WaitTimers();
    private ec = protractor.ExpectedConditions;

    private kendoLoadingMaskOff = this.ec.not(
        this.ec.presenceOf(element(by.className("k-loading-mask"))));

    waitForKendoGridData(columnName: protractor.ElementFinder) {
        console.log("Waiting for kendo grid to load");
        return browser.wait(this.kendoLoadingMaskOff, this.waitUpTo.twentySeconds)
            .then(() => this.waitForKendoGrid(columnName));
    }

    waitForKendoGrid(columnName: protractor.ElementFinder) {
        console.log(`Waiting for kendo grid to show column ${columnName}`);
        return browser
            .wait(this.ec.presenceOf(columnName), this.waitUpTo.twentySeconds);
    }
}
export = KendoLoaderHelper