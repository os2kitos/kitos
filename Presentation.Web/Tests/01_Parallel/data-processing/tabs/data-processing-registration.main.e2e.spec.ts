"use strict";
import Login = require("../../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import DataProcessingRegistrationOverviewPageObject = require("../../../PageObjects/Data-Processing/data-processing-registration.overview.po");
import WaitTimers = require("../../../Utility/WaitTimers");
import DataProcessingRegistrationEditMainPageObject = require("../../../PageObjects/Data-Processing/Tabs/data-processing-registration.edit.main.po");
import DataProcessingRegistrationHelper = require("../../../Helpers/DataProcessingRegistrationHelper");
import OrganizationHelper = require("../../../Helpers/OrgHelper");
import GetDateHelper = require("../../../Helpers/GetDateHelper");
import Select2Helper = require("../../../Helpers/Select2Helper");

describe("Data processing registration main detail tests", () => {

    const loginHelper = new Login();
    const pageObjectOverview = new DataProcessingRegistrationOverviewPageObject();
    const pageObject = new DataProcessingRegistrationEditMainPageObject();
    const testFixture = new TestFixtureWrapper();
    const waitUpTo = new WaitTimers();
    const ec = protractor.ExpectedConditions;
    const dpaHelper = DataProcessingRegistrationHelper;
    const orgHelper = OrganizationHelper;

    const dataProcessorName = "Fælles Kommune";

    const createName = (index: number) => {
        return `Dpa${new Date().getTime()}_${index}`;
    }

    const createCvr = () => {
        return `${new Date().getTime()}`.substring(1, 9);
    }

    const createRemark = (uniqueValue: string) => {
        return `Remark: ${new Date().getTime()} ${uniqueValue}`;
    }

    const dropdownYes = "Ja";
    const dropdownNo = "Nej";
    const thirdCountryName = "Danmark";
    const basisForTransfer = "Andet";
    const otherBasisForTransfer = "Intet";

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
            const dataResponsibleOptionName = "Fællesdataansvar";
            var dataResponsibleRemark = createRemark("dataResponsible");
            var agreementConcludedRemark = createRemark("agreementConcluded");

            dpaHelper.createAndOpenDataProcessingRegistration(name)
                //Changing name
                .then(() => renameNameAndVerify(renameValue))
                //Changing IsAgreementConcluded, AgreementConcludedAt and AgreementConcludedRemark
                .then(() => dpaHelper.changeIsAgreementConcluded(dropdownYes))
                .then(() => verifyIsAgreementConcluded(dropdownYes))
                .then(() => dpaHelper.changeAgreementConcludedAt(today))
                .then(() => verifyAgreementConcludedAt(today))
                .then(() => pageObject.getAgreementConcludedRemark().sendKeys(agreementConcludedRemark))
                .then(() => browser.waitForAngular())
                .then(() => verifyAgreementConcludedRemark(agreementConcludedRemark))
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
                .then(() => dpaHelper.assignSubDataProcessor(dataProcessorName, basisForTransfer, dropdownYes, thirdCountryName))
                .then(() => verifySubDataProcessorContent(dataProcessorName, basisForTransfer, dropdownYes, thirdCountryName))
                .then(() => dpaHelper.updateSubDataProcessor(dataProcessorName, otherBasisForTransfer, dropdownNo))
                .then(() => verifySubDataProcessorContent(dataProcessorName, otherBasisForTransfer, dropdownNo))
                .then(() => dpaHelper.removeSubDataProcessor(dataProcessorName))
                .then(() => expect(pageObject.getSubDataProcessorRow(dataProcessorName).isPresent()).toBeFalsy())
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

    it("Creating an organization with a special character and a cvr and verifying if dpr data processor and sub-data processor search allows for a special character and searching by cvr",
        () => {
            var dprName = createName(10);
            var organizationWithSpecialCharacterName = `August&Test${createName(10)}`;
            var organizationWithSpecialCharacterCvr = createCvr();

            loginHelper.logout()
                .then(() => loginHelper.loginAsGlobalAdmin())
                .then(() => orgHelper.createOrgWithCvr(organizationWithSpecialCharacterName, organizationWithSpecialCharacterCvr))
                .then(() => loginHelper.logout())
                .then(() => loginHelper.loginAsLocalAdmin())
                .then(() => dpaHelper.createDataProcessingRegistrationAndProceed(dprName))
                //assigning and verifying data processor with a special character
                .then(() => dpaHelper.assignDataProcessor(organizationWithSpecialCharacterName))
                .then(() => verifyDataProcessorContent([organizationWithSpecialCharacterName], []))
                .then(() => dpaHelper.removeDataProcessor(organizationWithSpecialCharacterName))
                .then(() => verifyDataProcessorContent([], [organizationWithSpecialCharacterName]))
                .then(() => dpaHelper.assignDataProcessor(organizationWithSpecialCharacterCvr))
                .then(() => verifyDataProcessorContent([organizationWithSpecialCharacterCvr], []))
                //assigning and verifying sub-data processor with a special character
                .then(() => dpaHelper.enableSubDataProcessors())
                .then(() => dpaHelper.verifyHasSubDataProcessorsToBeEnabled())
                .then(() => dpaHelper.assignSubDataProcessor(organizationWithSpecialCharacterName, basisForTransfer, dropdownYes, thirdCountryName))
                .then(() => verifySubDataProcessorContent(organizationWithSpecialCharacterName, basisForTransfer, dropdownYes, thirdCountryName))
                .then(() => dpaHelper.removeSubDataProcessor(organizationWithSpecialCharacterName))
                .then(() => expect(pageObject.getSubDataProcessorRow(organizationWithSpecialCharacterName).isPresent()).toBeFalsy())
                .then(() => dpaHelper.assignSubDataProcessor(organizationWithSpecialCharacterCvr, basisForTransfer, dropdownYes, thirdCountryName))
                .then(() => verifySubDataProcessorContent(organizationWithSpecialCharacterName, basisForTransfer, dropdownYes, thirdCountryName));
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

    function verifySubDataProcessorContent(sdpName: string, basisForTransfer: string, transferToInsecureThirdCountry: string, thirdCountryName?: string, cvr?: string) {
        expect(pageObject.getSubDataProcessorRow(sdpName).isPresent()).toBeTruthy();
        expect(pageObject.getSubDataProcessorCellWithContent(sdpName, basisForTransfer).isPresent()).toBeTruthy();
        expect(pageObject.getSubDataProcessorCellWithContent(sdpName, transferToInsecureThirdCountry).isPresent()).toBeTruthy();
        if (thirdCountryName) {
            expect(pageObject.getSubDataProcessorCellWithContent(sdpName, thirdCountryName).isPresent()).toBeTruthy();
        }
        if (cvr) {
            expect(pageObject.getSubDataProcessorCellWithContent(sdpName, cvr).isPresent()).toBeTruthy();
        }
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
            .then(() => browser.waitForAngular())
            .then(() => pageObject.getDpaMainNameInput().clear())
            .then(() => browser.waitForAngular())
            .then(() => pageObject.getDpaMainNameInput().sendKeys(name))
            .then(() => browser.waitForAngular())
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
        return setFocusOnNameToActivateBlur()
            .then(() => {
                console.log(`Expecting IsAgreementConcluded to be set to: ${selectedDate}`);
                expect(pageObject.getAgreementConcludedAtDateField().getAttribute("value")).toEqual(selectedDate);
            });
    }

    function verifyDataReponsible(selectedDataResponsible: string) {
        console.log(`Expecting DataReponsible to be set to: ${selectedDataResponsible}`);
        expect(Select2Helper.getData("s2id_dataResponsible_config").getText()).toEqual(selectedDataResponsible);
    }

    function verifyDataReponsibleRemark(dataResponsibleRemark: string) {
        return setFocusOnNameToActivateBlur().then(() => {
            console.log(`Expecting DataReponsibleRemark to be set to: ${dataResponsibleRemark}`);
            expect(pageObject.getDataResponsibleRemark().getAttribute("value")).toEqual(dataResponsibleRemark);
        });
    }

    function verifyAgreementConcludedRemark(agreementConcludedRemark: string) {
        return setFocusOnNameToActivateBlur().then(() => {
            console.log(`Expecting DataReponsibleRemark to be set to: ${agreementConcludedRemark}`);
            expect(pageObject.getAgreementConcludedRemark().getAttribute("value")).toEqual(agreementConcludedRemark);
        });
    }

    function setFocusOnNameToActivateBlur() {
        return pageObject.getDpaMainNameInput().click().then(() => browser.waitForAngular());
    }

});