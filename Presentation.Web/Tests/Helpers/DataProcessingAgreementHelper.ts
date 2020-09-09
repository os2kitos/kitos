import DataProcessingAgreementOverviewPageObject =
require("../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../Utility/WaitTimers");

class DataProcessingAgreementHelper {

    private static pageObject = new DataProcessingAgreementOverviewPageObject();
    private static waitUpTo = new WaitTimers();

    public static createDataProcessingAgreement(name: string) {
        return DataProcessingAgreementHelper.pageObject.getPage()
            .then(() => DataProcessingAgreementHelper.pageObject.waitForKendoGrid())
            .then(() => DataProcessingAgreementHelper.openNewDpaDialog())
            .then(() => DataProcessingAgreementHelper.enterDpaName(name))
            .then(() => DataProcessingAgreementHelper.validateSaveDpaClickable(true))
            .then(() => DataProcessingAgreementHelper.pageObject.getNewDpaSubmitButton().click())
            .then(() => DataProcessingAgreementHelper.pageObject.waitForKendoGrid());
    }

    public static goToSpecificDataProcessingAgreement(name: string) {
        console.log("Finding DataProcessingAgreement: " + name);
        return DataProcessingAgreementHelper.pageObject.getPage()
            .then(() => DataProcessingAgreementHelper.pageObject.waitForKendoGrid())
            .then(() => DataProcessingAgreementHelper.findDataProcessingAgreementColumnFor(name).first().click());
    } 

    private static openNewDpaDialog() {
        console.log("clicking createDpaButton");
        return DataProcessingAgreementHelper.pageObject.getCreateDpaButton().click()
            .then(() => {
                console.log("waiting for dialog to be visible");
                return browser.wait(DataProcessingAgreementHelper.pageObject.isCreateDpaAvailable(), DataProcessingAgreementHelper.waitUpTo.twentySeconds);
            });
    }

    private static validateSaveDpaClickable(isClickable: boolean) {
        console.log(`Expecting 'save' have clickable state equal ${isClickable}`);
        const expectation = expect(DataProcessingAgreementHelper.pageObject.getNewDpaSubmitButton().isEnabled());
        return isClickable ? expectation.toBeTruthy() : expectation.toBeFalsy();
    }

    private static enterDpaName(name: string) {
        console.log(`entering name: '${name}'`);
        return DataProcessingAgreementHelper.pageObject.getNewDpaNameInput().sendKeys(name);
    }

    private static findDataProcessingAgreementColumnFor(name: string) {
        return DataProcessingAgreementHelper.pageObject.kendoToolbarWrapper.getFilteredColumnElement(
            DataProcessingAgreementHelper.pageObject.kendoToolbarWrapper.columnObjects().dpaName,
            name);
    }
}

export = DataProcessingAgreementHelper;