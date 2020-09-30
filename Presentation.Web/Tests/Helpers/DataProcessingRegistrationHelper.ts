import DataProcessingRegistrationOverviewPageObject = require("../PageObjects/Data-Processing/data-processing-registration.overview.po");
import WaitTimers = require("../Utility/WaitTimers");
import LocalDataProcessing = require("../PageObjects/Local-admin/LocalDataProcessing.po");
import KendoToolbarWrapper = require("../Object-wrappers/KendoToolbarWrapper");
import NavigationHelper = require("../Utility/NavigationHelper");
import Select2Helper = require("./Select2Helper");
import DataProcessingRegistrationEditMainPageObject =
    require("../PageObjects/Data-Processing/Tabs/data-processing-registration.edit.main.po");

class DataProcessingRegistrationHelper {

    private static pageObject = new DataProcessingRegistrationOverviewPageObject();
    private static waitUpTo = new WaitTimers();
    private static kendoToolbarWrapper = new KendoToolbarWrapper();
    private static navigation = new NavigationHelper();
    private static editMainPo = new DataProcessingRegistrationEditMainPageObject();

    public static loadOverview() {
        return this.pageObject.getPage()
            .then(() => this.waitForKendo());
    }

    public static createDataProcessingRegistration(name: string) {
        console.log(`Creating registration with name ${name}`);
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

    public static goToSpecificDataProcessingRegistration(name: string) {
        console.log(`Finding registration: ${name}`);
        return this.pageObject.getPage()
            .then(() => this.pageObject.waitForKendoGrid())
            .then(() => this.findDataProcessingRegistrationColumnFor(name).first().click());
    }

    public static goToItSystems() {
        return DataProcessingRegistrationHelper.navigation.goToSubMenuElement("data-processing.edit-registration.it-systems");
    }

    public static goToRoles() {
        return DataProcessingRegistrationHelper.navigation.goToSubMenuElement("data-processing.edit-registration.roles");
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

    public static createAndOpenDataProcessingRegistration(name: string) {
        console.log(`Creating registration and navigating to ${name}`);
        return DataProcessingRegistrationHelper.createDataProcessingRegistration(name)
            .then(() => this.pageObject.findSpecificDpaInNameColumn(name))
            .then(() => DataProcessingRegistrationHelper.goToSpecificDataProcessingRegistration(name));
    }


    public static assignDataProcessor(name: string) {
        console.log("Assigning data processor with name: " + name);
        return Select2Helper.searchFor(name, "s2id_data-processor_select-new")
            .then(() => Select2Helper.waitForDataAndSelect());
    }

    public static removeDataProcessor(name: string) {
        console.log("Removing data processor with name: " + name);
        return this.editMainPo.getRemoveDataProcessorButton(name)
            .click()
            .then(() => browser.switchTo().alert().accept());
    }

    public static assignSubDataProcessor(name: string) {
        console.log("Assigning sub data processor with name: " + name);
        return Select2Helper.searchFor(name, "s2id_sub-data-processor_select-new")
            .then(() => Select2Helper.waitForDataAndSelect());
    }

    public static enableSubDataProcessors() {
        console.log("Enabling sub data processors");
        return Select2Helper.selectWithNoSearch("Ja", "s2id_hasSubDataProcessorsSelection");
    }

    public static  verifyHasSubDataProcessorsToBeEnabled() {
        console.log(`Expecting 'has sub data processors' to be set 'Ja'`);
        expect(Select2Helper.getData("s2id_hasSubDataProcessorsSelection").getText()).toEqual("Ja");
    }

    public static removeSubDataProcessor(name: string) {
        console.log("Removing sub data processor with name: " + name);
        return this.editMainPo.getRemoveSubDataProcessorButton(name)
            .click()
            .then(() => browser.switchTo().alert().accept());
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

    public static changeIsAgreementConcluded(changeToValue: string) {
        console.log("Changing IsAgreementConcluded to: " + changeToValue);
        return Select2Helper.selectWithNoSearch(changeToValue, "s2id_agreementConcluded");
    }

    public static changeAgreementConcludedAt(changeToDate: string) {
        console.log("Changing AgreementConcludedAt to date: " + changeToDate);
        return this.editMainPo.getAgreementConcludedAtDateField().sendKeys(changeToDate);
    }

    public static changeOversightInterval(changeToInterval: string) {
        console.log(`Changing Oversight Interval to ${changeToInterval}`);
        return Select2Helper.selectWithNoSearch(changeToInterval, "s2id_oversightInterval");
    }

    private static validateSaveDpaClickable(isClickable: boolean) {
        console.log(`Expecting 'save' have clickable state equal ${isClickable}`);
        const expectation = expect(this.pageObject.getNewDpaSubmitButton().isEnabled());
        return isClickable ? expectation.toBeTruthy() : expectation.toBeFalsy();
    }

    private static findDataProcessingRegistrationColumnFor(name: string) {
        return this.kendoToolbarWrapper.getFilteredColumnElement(
            this.kendoToolbarWrapper.columnObjects().dpaName,
            name);
    }

    private static waitForKendo() {
        console.log("waiting for kendo grid to load");
        return this.pageObject.waitForKendoGrid();
    }
}

export = DataProcessingRegistrationHelper;