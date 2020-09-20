import DataProcessingAgreementOverviewPageObject = require("../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../Utility/WaitTimers");
import LocalDataProcessing = require("../PageObjects/Local-admin/LocalDataProcessing.po");
import KendoToolbarWrapper = require("../Object-wrappers/KendoToolbarWrapper");
import NavigationHelper = require("../Utility/NavigationHelper");
import Select2Helper = require("./Select2Helper");

class DataProcessingAgreementHelper {

    private static pageObject = new DataProcessingAgreementOverviewPageObject();
    private static waitUpTo = new WaitTimers();
    private static kendoToolbarWrapper = new KendoToolbarWrapper();
    private static navigation = new NavigationHelper();

    public static loadOverview() {
        return this.pageObject.getPage()
            .then(() => this.waitForKendo());
    }

    public static createDataProcessingAgreement(name: string) {
        console.log(`Creating agreement with name ${name}`);
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

    public static goToItSystems() {
        return DataProcessingAgreementHelper.navigation.goToSubMenuElement("data-processing.edit-agreement.it-systems");
    }

    public static goToRoles() {
        return DataProcessingAgreementHelper.navigation.goToSubMenuElement("data-processing.edit-agreement.roles");
    }

    public static assignRole(role: string, user: string) {
        console.log("Assigning role: " + role + " to user: " + user);
        return Select2Helper.searchFor(role, "s2id_add-role")
            .then(() => Select2Helper.waitForDataAndSelect())
            .then(() => Select2Helper.searchFor(user, "s2id_add-user"))
            .then(() => Select2Helper.waitForDataAndSelect());
    }

    public static removeRole(roleName: string, userName: string) {
        console.log("Removing role: ${role} with user: ${user}");
        return this.pageObject.getRoleDeleteButton(roleName, userName)
            .then(element => element.click())
            .then(() => browser.switchTo().alert().accept());
    }

    public static editRole(oldRoleName: string, oldUserName: string, role: string, user: string) {
        // Can only edit one role at a time. Otherwise the getRoleSubmitButton method fails to find the correct button.
        console.log(`Editing role: ${role} with user: ${user}`);
        return this.pageObject.getRoleRow(oldRoleName, oldUserName).then(row => {
            return this.pageObject.getRoleEditButton(oldRoleName, oldUserName)
                .then(element => element.click())
                .then(() => Select2Helper.searchForByParent(role, "s2id_edit-role", row))
                .then(() => Select2Helper.waitForDataAndSelect())
                .then(() => Select2Helper.searchForByParent(user, "s2id_edit-user", row))
                .then(() => Select2Helper.waitForDataAndSelect())
                .then(() => this.pageObject.getRoleSubmitEditButton().click());
        });
    }

    public static assignSystem(name: string) {
        console.log("Assigning system with name: " + name);
        return Select2Helper.searchFor(name, "s2id_select-new-system")
            .then(() => Select2Helper.waitForDataAndSelect());
    }

    public static removeSystem(name: string) {
        console.log("Removing system with name: " + name);
        return this.pageObject.getRemoveSystemButton(name)
            .click()
            .then(() => browser.switchTo().alert().accept());
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
        return this.kendoToolbarWrapper.getFilteredColumnElement(
            this.kendoToolbarWrapper.columnObjects().dpaName,
            name);
    }

    private static waitForKendo() {
        console.log("waiting for kendo grid to load");
        return this.pageObject.waitForKendoGrid();
    }
}

export = DataProcessingAgreementHelper;