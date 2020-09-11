import DataProcessingAgreementOverviewPageObject = require("../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../Utility/WaitTimers");
import LocalDataProcessing = require("../PageObjects/Local-admin/LocalDataProcessing.po");

class DataProcessingAgreementHelper {

    private static pageObject = new DataProcessingAgreementOverviewPageObject();
    private static waitUpTo = new WaitTimers();

    public static createDataProcessingAgreement(name: string) {
        console.log("Creating agreement with name " + name);
        return this.pageObject.getPage()
            .then(() => this.waitForKendo())
            .then(() => this.openNewDpaDialog())
            .then(() => this.enterDpaName(name))
            .then(() => {
                console.log(`clicking 'save'`);
                this.validateSaveDpaClickable(true);
            })
            .then(() => this.pageObject.getNewDpaSubmitButton().click())
            .then(() => this.pageObject.waitForKendoGrid());
    }

    public static checkAndEnableDpaModule() {
        var localDpPo = new LocalDataProcessing();
        return localDpPo
            .getPage()
            .then(() => localDpPo.getToggleDataProcessingCheckbox().isSelected())
            .then((selected) => {
                if (!selected) {
                    localDpPo.getToggleDataProcessingCheckbox().click();
                }
            });
    }

    public static goToSpecificDataProcessingAgreement(name: string) {
        console.log(`Finding DataProcessingAgreement: ${name}`);
        return this.pageObject.getPage()
            .then(() => this.pageObject.waitForKendoGrid())
            .then(() => this.findDataProcessingAgreementColumnFor(name).first().click());
    }

    public static openNewDpaDialog() {
        console.log("clicking createDpaButton");
        return this.pageObject.getCreateDpaButton().click()
            .then(() => {
                console.log("waiting for dialog to be visible");
                return browser.wait(this.pageObject.isCreateDpaAvailable(), this.waitUpTo.twentySeconds);
            });
    }

    public static enterDpaName(name: string) {
        console.log(`entering name: '${name}'`);
        return this.pageObject.getNewDpaNameInput().sendKeys(name);
    }

    private static validateSaveDpaClickable(isClickable: boolean) {
        console.log(`Expecting 'save' have clickable state equal ${isClickable}`);
        const expectation = expect(this.pageObject.getNewDpaSubmitButton().isEnabled());
        return isClickable ? expectation.toBeTruthy() : expectation.toBeFalsy();
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