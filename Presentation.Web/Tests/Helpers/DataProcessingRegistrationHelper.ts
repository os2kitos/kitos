﻿import DataProcessingRegistrationOverviewPageObject = require("../PageObjects/Data-Processing/data-processing-registration.overview.po");
import WaitTimers = require("../Utility/WaitTimers");
import LocalDataProcessing = require("../PageObjects/Local-admin/LocalDataProcessing.po");
import KendoToolbarWrapper = require("../Object-wrappers/KendoToolbarWrapper");
import NavigationHelper = require("../Utility/NavigationHelper");
import Select2Helper = require("./Select2Helper");
import DataProcessingRegistrationEditMainPageObject =
require("../PageObjects/Data-Processing/Tabs/data-processing-registration.edit.main.po");
import DataProcessingRegistrationEditOversightPageObject =
require("../PageObjects/Data-Processing/Tabs/data-processing-registration.edit.oversight.po");

class DataProcessingRegistrationHelper {
    private static readonly selectBasisForTransferSelectionId: string = "s2id_basisForTransfer_config";
    private static readonly transferToThirdCountriesSelectionId: string = "s2id_transferToInsecureThirdCountries_config";
    private static pageObject = new DataProcessingRegistrationOverviewPageObject();
    private static waitUpTo = new WaitTimers();
    private static kendoToolbarWrapper = new KendoToolbarWrapper();
    private static navigation = new NavigationHelper();
    private static editMainPo = new DataProcessingRegistrationEditMainPageObject();
    private static editOversightPo = new DataProcessingRegistrationEditOversightPageObject();

    public static loadOverview() {
        return this.pageObject.getPage()
            .then(() => browser.waitForAngular())
            .then(() => this.waitForKendo());
    }

    public static createDataProcessingRegistration(name: string) {
        console.log(`Creating registration with name ${name}`);
        return this.pageObject.getPage()
            .then(() => browser.waitForAngular())
            .then(() => this.waitForKendoGridControls())
            .then(() => this.openNewDpaDialog())
            .then(() => this.enterDpaName(name))
            .then(() => {
                console.log(`clicking 'save'`);
                this.validateSaveDpaClickable(true);
            })
            .then(() => this.pageObject.getNewDpaSubmitButton().click())
            .then(() => browser.waitForAngular())
            .then(() => this.pageObject.waitForKendoGrid());
    }

    public static createDataProcessingRegistrationAndProceed(name: string) {
        console.log(`Creating registration with name ${name}`);
        return this.pageObject.getPage()
            .then(() => browser.waitForAngular())
            .then(() => this.waitForKendoGridControls())
            .then(() => this.openNewDpaDialog())
            .then(() => this.enterDpaName(name))
            .then(() => {
                console.log(`clicking 'save'`);
                this.validateSaveDpaClickable(true);
            })
            .then(() => console.log("Proceed to registration"))
            .then(() => this.pageObject.getNewDpaSubmitAndProceedButton().click())
            .then(() => browser.waitForAngular())
    }

    public static checkAndEnableDpaModule() {
        var localDpPo = new LocalDataProcessing();
        return localDpPo
            .getPage()
            .then(() => localDpPo.getToggleDataProcessingCheckbox().isSelected())
            .then((selected) => {
                if (!selected) {
                    return localDpPo.getToggleDataProcessingCheckbox().click();
                }
                return "nop";
            })
            .then(() => browser.waitForAngular());
    }

    public static goToSpecificDataProcessingRegistration(name: string) {
        console.log(`Finding registration: ${name}`);
        return this.pageObject.getPage()
            .then(() => this.pageObject.waitForKendoGrid())
            .then(() => this.findDataProcessingRegistrationColumnFor(name).first().click())
            .then(() => browser.waitForAngular());
    }

    public static goToItSystems() {
        return DataProcessingRegistrationHelper.navigation.goToSubMenuElement("data-processing.edit-registration.it-systems");
    }

    public static goToRoles() {
        return DataProcessingRegistrationHelper.navigation.goToSubMenuElement("data-processing.edit-registration.roles");
    }

    public static goToItContracts() {
        return DataProcessingRegistrationHelper.navigation.goToSubMenuElement("data-processing.edit-registration.contracts");
    }

    public static assignRole(role: string, user: string) {
        console.log("Assigning role: " + role + " to user: " + user);
        return Select2Helper.selectWithNoSearch(role, "s2id_add-role")
            .then(() => Select2Helper.searchFor(user, "s2id_add-user"))
            .then(() => Select2Helper.waitForDataAndSelect());
    }

    public static removeRole(roleName: string, userName: string) {
        console.log("Removing role: ${role} with user: ${user}");
        return this.pageObject.getRoleDeleteButton(roleName, userName)
            .then(element => element.click())
            .then(() => browser.switchTo().alert().accept())
            .then(() => browser.waitForAngular());
    }

    public static editRole(oldRoleName: string, oldUserName: string, role: string, user: string) {
        // Can only edit one role at a time. Otherwise the getRoleSubmitButton method fails to find the correct button.
        console.log(`Editing role: ${role} with user: ${user}`);
        return this.pageObject.getRoleRow(oldRoleName, oldUserName).then(row => {
            return this.pageObject.getRoleEditButton(oldRoleName, oldUserName)
                .then(element => element.click())
                .then(() => Select2Helper.selectNoSearchByParent(role, "s2id_edit-role", row))
                .then(() => Select2Helper.searchForByParent(user, "s2id_edit-user", row))
                .then(() => Select2Helper.waitForDataAndSelect())
                .then(() => this.pageObject.getRoleSubmitEditButton().click())
                .then(() => browser.waitForAngular());
        });
    }

    public static createAndOpenDataProcessingRegistration(name: string) {
        console.log(`Creating registration and navigating to ${name}`);
        return DataProcessingRegistrationHelper.createDataProcessingRegistration(name)
            .then(() => this.pageObject.findSpecificDpaInNameColumn(name))
            .then(() => DataProcessingRegistrationHelper.goToSpecificDataProcessingRegistration(name));
    }

    public static createAndProceedDataProcessingRegistration(name: string) {
        console.log(`Creating registration and navigating to ${name}`);
        return DataProcessingRegistrationHelper.createDataProcessingRegistrationAndProceed(name);
    }

    public static assignDataProcessor(name: string) {
        console.log("Assigning data processor with name: " + name);
        return Select2Helper.searchFor(name, "s2id_data-processor_select-new_config")
            .then(() => Select2Helper.waitForDataAndSelect());
    }

    public static removeDataProcessor(name: string) {
        console.log("Removing data processor with name: " + name);
        return this.editMainPo.getRemoveDataProcessorButton(name)
            .click()
            .then(() => browser.switchTo().alert().accept())
            .then(() => browser.waitForAngular());
    }

    public static assignSubDataProcessor(name: string, basisForTransfer: string, dropdownYes: string, thirdCountryName: string) {
        
        console.log("Assigning sub data processor with name: " + name);
        return this.editMainPo.getAddSubDataProcessorButton().click()
            .then(() => Select2Helper.searchFor(name, "s2id_sub-data-processor_select-new_config"))
            .then(() => Select2Helper.waitForDataAndSelect())
            .then(() => Select2Helper.selectWithNoSearch(basisForTransfer, "s2id_basisForTransferConfig_config"))
            .then(() => Select2Helper.selectWithNoSearch(dropdownYes, "s2id_transferToInsecureThirdCountries_config"))
            .then(() => Select2Helper.selectWithNoSearch(thirdCountryName, "s2id_insecureThirdCountryConfig_config"))
            .then(() => this.editMainPo.getSaveSubDataProcessorButton().click());
    }

    public static updateSubDataProcessor(name: string, basisForTransfer: string, dropdownNo: string) {
        
        console.log("Assigning sub data processor with name: " + name);
        return this.editMainPo.getEditSubDataProcessorButton(name).click()
            .then(() => Select2Helper.selectWithNoSearch(basisForTransfer, "s2id_basisForTransferConfig_config"))
            .then(() => Select2Helper.selectWithNoSearch(dropdownNo, "s2id_transferToInsecureThirdCountries_config"))
            .then(() => this.editMainPo.getSaveSubDataProcessorButton().click());
    }

    public static enableSubDataProcessors() {
        console.log("Enabling sub data processors");
        return Select2Helper.selectWithNoSearch("Ja", "s2id_hasSubDataProcessorsSelection_config");
    }

    public static verifyHasSubDataProcessorsToBeEnabled() {
        console.log(`Expecting 'has sub data processors' to be set 'Ja'`);
        expect(Select2Helper.getData("s2id_hasSubDataProcessorsSelection_config").getText()).toEqual("Ja");
    }

    public static removeSubDataProcessor(name: string) {
        console.log("Removing sub data processor with name: " + name);
        return this.editMainPo.getRemoveSubDataProcessorButton(name)
            .click()
            .then(() => browser.switchTo().alert().accept())
            .then(() => browser.waitForAngular());
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
            .then(() => browser.switchTo().alert().accept())
            .then(() => browser.waitForAngular());
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
        return Select2Helper.selectWithNoSearch(changeToValue, "s2id_agreementConcluded_config");
    }

    public static changeAgreementConcludedAt(changeToDate: string) {
        console.log("Changing AgreementConcludedAt to date: " + changeToDate);
        return this.editMainPo.getAgreementConcludedAtDateField().sendKeys(changeToDate);
    }

    public static changeOversightInterval(changeToInterval: string) {
        console.log(`Changing Oversight Interval to ${changeToInterval}`);
        return Select2Helper.selectWithNoSearch(changeToInterval, "s2id_oversightInterval_config");
    }

    public static changeOversightCompleted(changeToCompleted: string) {
        console.log(`Changing Oversight Completed to ${changeToCompleted}`);
        return Select2Helper.selectWithNoSearch(changeToCompleted, "s2id_oversightCompleted_config");
    }

    public static assignOversightDateAndRemark(date: string, remark: string) {
        console.log(`Assigning oversight date to {date} and remark to {remark}`);
        return this.editOversightPo.getAssignOversightDateButton().click()
            .then(() => this.editOversightPo.getOversightDateModalDateField().sendKeys(date))
            .then(() => this.editOversightPo.getOversightDateModalRemarkField().sendKeys(remark))
            .then(() => this.editOversightPo.getOversightDateModalSaveButton().click())
            .then(() => browser.waitForAngular());
    }

    public static verifyOversightDateAndRemark(date: string, remark: string) {
        console.log(`Verifying oversight date set to {date} and remark set to {remark}`);
        expect(this.editOversightPo.getOversightDateRowDate().getAttribute("value")).toEqual(date);
        expect(this.editOversightPo.getOversightDateRowRemark().getAttribute("value")).toEqual(remark);
    }

    public static removeOversightDateAndRemark() {
        console.log(`Removing oversight date`);
        return this.editOversightPo.getRemoveOversightDateButton().click()
            .then(() => browser.switchTo().alert().accept())
            .then(() => browser.waitForAngular());
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

    private static findAnyDataProcessingRegistrationColumn() {
        return this.kendoToolbarWrapper.getAnyColumnElement(this.kendoToolbarWrapper.columnObjects().dpaName);
    }

    private static waitForKendo() {
        console.log("waiting for kendo grid to load");
        return this.pageObject.waitForKendoGrid();
    }

    private static waitForKendoGridControls() {
        console.log("waiting for kendo grid to load");
        return this.pageObject.waitForKendoGridControls();
    }

    public static enableTransferToInsecureThirdCountries() {
        console.log("Enabling transfer to unsafe third countries");
        return Select2Helper.selectWithNoSearch("Ja", DataProcessingRegistrationHelper.transferToThirdCountriesSelectionId);
    }

    public static verifyHasTransferToInsecureThirdCountriesToBeEnabled() {
        console.log(`Expecting 'transfer to unsafe third countries' to be set 'Ja'`);
        expect(Select2Helper.getData(DataProcessingRegistrationHelper.transferToThirdCountriesSelectionId).getText()).toEqual("Ja");
    }

    public static assignThirdCountry(thirdCountryName) {
        console.log(`Assigning unsafe third country with name: ${thirdCountryName}`);
        return Select2Helper.selectWithNoSearch(thirdCountryName, "s2id_insecure-third-country_select-new_config");
    }

    public static removeThirdCountry(thirdCountryName) {
        console.log(`Removing unsafe third country with name: ${thirdCountryName}`);
        return this.editMainPo.getRemoveThirdCountryButton(thirdCountryName)
            .click()
            .then(() => browser.switchTo().alert().accept())
            .then(() => browser.waitForAngular());
    }

    public static assignDataResponsible(dataResponsibleOptionName) {
        console.log(`Assigning data responsible option with name: ${dataResponsibleOptionName}`);
        return Select2Helper.selectWithNoSearch(dataResponsibleOptionName, "s2id_dataResponsible_config");
    }

    public static assignOversightOption(name: string) {
        console.log("Assigning oversight option with name: " + name);
        return Select2Helper.selectWithNoSearch(name, "s2id_oversight-option_select-new_config");
    }

    public static removeOversightOption(name: string) {
        console.log("Removing oversight option with name: " + name);
        return this.editOversightPo.getRemoveOversightOptionButton(name)
            .click()
            .then(() => browser.switchTo().alert().accept())
            .then(() => browser.waitForAngular());
    }

    static selectBasisForTransfer(basisForTransfer: string) {
        return Select2Helper.selectWithNoSearch(basisForTransfer, DataProcessingRegistrationHelper.selectBasisForTransferSelectionId);
    }

    static verifyBasisForTransfer(basisForTransfer: string) {
        expect(Select2Helper.getData(DataProcessingRegistrationHelper.selectBasisForTransferSelectionId).getText()).toEqual(basisForTransfer);
    }

    static clickSystem(systemName: string) { return element(by.linkText(systemName)).click() };

    static clickContract(contractName: string) { return element(by.linkText(contractName)).click() };
}

export = DataProcessingRegistrationHelper;