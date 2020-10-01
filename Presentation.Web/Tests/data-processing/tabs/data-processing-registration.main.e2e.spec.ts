"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingRegistrationOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-registration.overview.po");
import WaitTimers = require("../../Utility/WaitTimers");
import DataProcessingRegistrationEditMainPageObject = require("../../PageObjects/Data-Processing/Tabs/data-processing-registration.edit.main.po");
import DataProcessingRegistrationHelper = require("../../Helpers/DataProcessingRegistrationHelper");
import GetDateHelper = require("../../Helpers/GetDateHelper");
import Select2Helper = require("../../Helpers/Select2Helper");

describe("Data processing agreement main detail tests", () => {

    const loginHelper = new Login();
    const pageObjectOverview = new DataProcessingRegistrationOverviewPageObject();
    const pageObject = new DataProcessingRegistrationEditMainPageObject();
    const testFixture = new TestFixtureWrapper();
    const waitUpTo = new WaitTimers();
    const ec = protractor.ExpectedConditions;
    const dpaHelper = DataProcessingRegistrationHelper;
    const dataProcessorName = "Fælles Kommune";

    const createName = (index: number) => {
        return `Dpa${new Date().getTime()}_${index}`;
    }

    const createRemark = () => {
        return `Remark: ${new Date().getTime()}`;
    }

    var dropdownYes = "Ja";

    var today = GetDateHelper.getTodayAsString();

    beforeAll(() => {
        loginHelper.loginAsLocalAdmin();
        testFixture.enableLongRunningTest();
        dpaHelper.checkAndEnableDpaModule();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });


    it("Creating and modifying, and deleting data processing registration",
        () => {
            var name = createName(10);
            var renameValue = createName(30);
            const thirdCountryName = "Danmark";
            const basisForTransfer = "Andet";
            const dataResponsibleOptionName = "";
            var dataResponsibleRemark = createRemark();

            dpaHelper.createDataProcessingRegistration(name)
                //Changing name
                .then(() => pageObjectOverview.findSpecificDpaInNameColumn(name))
                .then(() => dpaHelper.goToSpecificDataProcessingRegistration(name))
                .then(() => renameNameAndVerify(renameValue))
                //Changing IsAgreementConcluded and AgreementConcludedAt
                .then(() => dpaHelper.changeIsAgreementConcluded(dropdownYes))
                .then(() => verifyIsAgreementConcluded(dropdownYes))
                .then(() => dpaHelper.changeAgreementConcludedAt(today))
                .then(() => verifyAgreementConcludedAt(today))
                //Changing data responsible
                .then(() => dpaHelper.assignDataResponsible(dataResponsibleOptionName))
                .then(() => verifyDataReponsible(dataResponsibleOptionName))
                .then(() => pageObject.getDataResponsibleRemark().sendKeys(dataResponsibleRemark))
                .then(() => verifyDataReponsibleRemark(dataResponsibleRemark))
                //Changing data processors
                .then(() => dpaHelper.assignDataProcessor(dataProcessorName))
                .then(() => verifyDataProcessorContent([dataProcessorName], []))
                .then(() => dpaHelper.removeDataProcessor(dataProcessorName))
                .then(() => verifyDataProcessorContent([], [dataProcessorName]))
                //Changing sub data processors
                .then(() => dpaHelper.enableSubDataProcessors())
                .then(() => dpaHelper.verifyHasSubDataProcessorsToBeEnabled())
                .then(() => dpaHelper.assignSubDataProcessor(dataProcessorName))
                .then(() => verifySubDataProcessorContent([dataProcessorName], []))
                .then(() => dpaHelper.removeSubDataProcessor(dataProcessorName))
                .then(() => verifySubDataProcessorContent([], [dataProcessorName]))
                //Changing transfer to insecure third countries
                .then(() => dpaHelper.enableTransferToInsecureThirdCountries())
                .then(() => dpaHelper.verifyHasTransferToInsecureThirdCountriesToBeEnabled())
                .then(() => dpaHelper.assignThirdCountry(thirdCountryName))
                .then(() => verifyThirdCountrySelectionContent([thirdCountryName], []))
                .then(() => dpaHelper.removeThirdCountry(thirdCountryName))
                .then(() => verifyThirdCountrySelectionContent([], [thirdCountryName]))
                //Changing basisfortransfer
                .then(() => dpaHelper.selectBasisForTransfer(basisForTransfer))
                .then(() => dpaHelper.verifyBasisForTransfer(basisForTransfer))
                //COMPLETE - Delete the registration and verify
                .then(() => getDeleteButtonAndDelete())
                .then(() => dpaHelper.loadOverview())
                .then(() => expect(pageObjectOverview.findSpecificDpaInNameColumn(renameValue).isPresent()).toBeFalsy());
        });


    function verifyDataProcessorContent(presentNames: string[], unpresentNames: string[]) {
        presentNames.forEach(name => {
            console.log(`Expecting data procesor to be present:${name}`);
            expect(pageObject.getDataProcessorRow(name).isPresent()).toBeTruthy();
        });
        unpresentNames.forEach(name => {
            console.log(`Expecting data processor NOT to be present:${name}`);
            expect(pageObject.getDataProcessorRow(name).isPresent()).toBeFalsy();
        });
    }

    function verifySubDataProcessorContent(presentNames: string[], unpresentNames: string[]) {
        presentNames.forEach(name => {
            console.log(`Expecting sub data processor to be present:${name}`);
            expect(pageObject.getSubDataProcessorRow(name).isPresent()).toBeTruthy();
        });
        unpresentNames.forEach(name => {
            console.log(`Expecting sub data processor NOT to be present:${name}`);
            expect(pageObject.getSubDataProcessorRow(name).isPresent()).toBeFalsy();
        });
    }

    function verifyThirdCountrySelectionContent(presentNames: string[], unpresentNames: string[]) {
        presentNames.forEach(name => {
            console.log(`Expecting country to be present:${name}`);
            expect(pageObject.getThirdCountryProcessorRow(name).isPresent()).toBeTruthy();
        });
        unpresentNames.forEach(name => {
            console.log(`Expecting country NOT to be present:${name}`);
            expect(pageObject.getThirdCountryProcessorRow(name).isPresent()).toBeFalsy();
        });
    }

    function renameNameAndVerify(name: string) {
        console.log(`Renaming registration to ${name}`);
        return pageObject.getDpaMainNameInput().click()
            .then(() => pageObject.getDpaMainNameInput().clear())
            .then(() => pageObject.getDpaMainNameInput().sendKeys(name))
            .then(() => pageObject.getDpaMainNameInput().sendKeys(protractor.Key.TAB))
            .then(() => {
                console.log(`Expecting registration to be called ${name}`);
                browser.wait(ec.textToBePresentInElement(pageObject.getDpaMainNameHeader(), name),
                    waitUpTo.twentySeconds,
                    `Could not verify that ${name} was changed`);
            });
    }

    function getDeleteButtonAndDelete() {
        console.log("Retrieving deletebutton");
        return pageObject.getDpaDeleteButton().click()
            .then(() => browser.switchTo().alert().accept());
    }

    function verifyIsAgreementConcluded(selectedValue: string) {
        console.log(`Expecting IsAgreementConcluded to be set to: ${selectedValue}`);
        expect(Select2Helper.getData("s2id_agreementConcluded_config").getText()).toEqual(selectedValue);
    }

    function verifyAgreementConcludedAt(selectedDate: string) {
        setFocusOnNameToActivateBlur();
        console.log(`Expecting IsAgreementConcluded to be set to: ${selectedDate}`);
        expect(pageObject.getAgreementConcludedAtDateField().getAttribute("value")).toEqual(selectedDate);
    }

    function verifyDataReponsible(selectedDataResponsible: string) {
        console.log(`Expecting DataReponsible to be set to: ${selectedDataResponsible}`);
        expect(Select2Helper.getData("s2id_dataResponsible_config").getText()).toEqual(selectedDataResponsible);
    }

    function verifyDataReponsibleRemark(dataResponsibleRemark: string) {
        setFocusOnNameToActivateBlur();
        console.log(`Expecting DataReponsibleRemark to be set to: ${dataResponsibleRemark}`);
        expect(pageObject.getDataResponsibleRemark().getText()).toEqual(dataResponsibleRemark);
    }

    function setFocusOnNameToActivateBlur() {
        return pageObject.getDpaMainNameInput().click();
    }

});