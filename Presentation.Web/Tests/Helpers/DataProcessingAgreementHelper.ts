import DataProcessingAgreementOverviewPageObject =
require("../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../Utility/WaitTimers");

class DataProcessingAgreementHelper {

    private static pageObject = new DataProcessingAgreementOverviewPageObject();
    private static waitUpTo = new WaitTimers();
    private static ec = protractor.ExpectedConditions;

    public static createDataProcessingAgreement(name: string) {
        console.log("Creating agreement with name " + name);
        return this.pageObject.getPage()
            .then(() => this.waitForKendo())
            .then(() => this.openNewDpaDialog())
            .then(() => this.enterDpaName(name))
            .then(() => this.validateSaveDpaClickable(true))
            .then(() => this.pageObject.getNewDpaSubmitButton().click())
            .then(() => this.pageObject.waitForKendoGrid());
    }

    public static goToSpecificDataProcessingAgreement(name: string) {
        console.log("Finding DataProcessingAgreement: " + name);
        return this.pageObject.getPage()
            .then(() => this.pageObject.waitForKendoGrid())
            .then(() => this.findDataProcessingAgreementColumnFor(name).first().click());
    }

    private static openNewDpaDialog() {
        console.log("clicking createDpaButton");
        return this.pageObject.getCreateDpaButton().click()
            .then(() => {
                console.log("waiting for dialog to be visible");
                return browser.wait(this.pageObject.isCreateDpaAvailable(), this.waitUpTo.twentySeconds);
            });
    }

    private static validateSaveDpaClickable(isClickable: boolean) {
        console.log(`Expecting 'save' have clickable state equal ${isClickable}`);
        const expectation = expect(this.pageObject.getNewDpaSubmitButton().isEnabled());
        return isClickable ? expectation.toBeTruthy() : expectation.toBeFalsy();
    }

    private static enterDpaName(name: string) {
        console.log(`entering name: '${name}'`);
        return this.pageObject.getNewDpaNameInput().sendKeys(name);
    }

    private static findDataProcessingAgreementColumnFor(name: string) {
        return this.pageObject.kendoToolbarWrapper.getFilteredColumnElement(
            this.pageObject.kendoToolbarWrapper.columnObjects().dpaName,
            name);
    }

    private static waitForKendo() {
        console.log("waiting for kendo grid to load");
        return this.pageObject.waitForKendoGrid();
    }
}

export = DataProcessingAgreementHelper;