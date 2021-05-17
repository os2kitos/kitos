"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingRegistrationEditOversightPageObject = require("../../PageObjects/Data-Processing/Tabs/data-processing-registration.edit.oversight.po");
import DataProcessingRegistrationHelper = require("../../Helpers/DataProcessingRegistrationHelper");
import Select2Helper = require("../../Helpers/Select2Helper");
import DateHelper = require("../../Helpers/GetDateHelper");

describe("Data processing registration oversight detail tests", () => {

    const loginHelper = new Login();
    const pageObject = new DataProcessingRegistrationEditOversightPageObject();
    const testFixture = new TestFixtureWrapper();
    const dpaHelper = DataProcessingRegistrationHelper;

    const createName = () => {
        return `Dpa_${new Date().getTime()}`;
    }

    const createRemark = () => {
        return `OversightRemark_${new Date().getTime()}`;
    }

    beforeAll(() => {
        loginHelper.loginAsLocalAdmin();
        testFixture.enableLongRunningTest();
        dpaHelper.checkAndEnableDpaModule();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("Is able to set oversight data",
        () => {
            const name = createName();
            const intervalRemark = createRemark();
            const completedRemark = createRemark();
            const dropdownInterval = "Hver andet år";
            const oversightOptionName = "Egen kontrol";
            const oversightOptionRemark = createRemark();
            const oversightDateRemark = createRemark();
            const dropdownCompleted = "Ja";
            const oversightDate = DateHelper.getTodayAsString();

            // Creating and navigating to DPR
            dpaHelper.createAndOpenDataProcessingRegistration(name)
                .then(() => pageObject.getOversightPage())
                .then(() => pageObject.getOversightIntervalRemark().sendKeys(intervalRemark))
                .then(() => dpaHelper.changeOversightInterval(dropdownInterval))
                .then(() => verifyOversightInterval(dropdownInterval))
                .then(() => verifyOversightIntervalRemark(intervalRemark))
                // Oversight options
                .then(() => dpaHelper.assignOversightOption(oversightOptionName))
                .then(() => verifyOversightOptionContent([oversightOptionName], []))
                .then(() => dpaHelper.removeOversightOption(oversightOptionName))
                .then(() => verifyOversightOptionContent([], [oversightOptionName]))
                // Oversight option remark
                .then(() => pageObject.getOversightOptionRemark().sendKeys(oversightOptionRemark))
                .then(() => verifyOversightOptionRemark(intervalRemark))
                // Oversight is completed
                .then(() => expectOversightCompletedLatestDateVisibility(false))
                .then(() => dpaHelper.changeOversightCompleted(dropdownCompleted))
                .then(() => pageObject.getOversightCompletedRemark().sendKeys(completedRemark))
                .then(() => verifyOversightCompletedRemark(completedRemark))
                .then(() => verifyOversightCompleted(dropdownCompleted))
                .then(() => expectOversightCompletedLatestDateVisibility(true))
                // Oversight dates
                .then(() => dpaHelper.assignOversightDateAndRemark(oversightDate, oversightDateRemark))
                .then(() => dpaHelper.verifyOversightDateAndRemark(oversightDate, oversightDateRemark))
                .then(() => dpaHelper.removeOversightDateAndRemark())
                .then(() => verifyOversightDateAndRemarkRemoved());
        });


    function verifyOversightCompleted(selectedValue: string) {
        console.log(`Expecting oversight completed to be set to: ${selectedValue}`);
        activateBlur()
            .then(() => expect(Select2Helper.getData("s2id_oversightCompleted_config").getText()).toEqual(selectedValue));
    }

    function verifyOversightInterval(selectedValue: string) {
        console.log(`Expecting oversight completed to be set to: ${selectedValue}`);
        activateBlur()
            .then(() => expect(Select2Helper.getData("s2id_oversightInterval_config").getText()).toEqual(selectedValue));

    }

    function verifyOversightIntervalRemark(expectedValue: string) {
        console.log(`Expecting oversight interval remark to be set to: ${expectedValue}`);
        activateBlur()
            .then(() => expect(pageObject.getOversightIntervalRemark().getAttribute("value")).toEqual(expectedValue));
    }

    function expectOversightCompletedLatestDateVisibility(visible: boolean) {
        console.log(`Expecting visiblity of oversight completed date to be set to: ${visible}`);
        activateBlur()
            .then(() => expect(pageObject.getAssignOversightDateButton().isPresent()).toBe(visible));
    }

    function verifyOversightOptionRemark(expectedValue: string) {
        console.log(`Expecting oversight option remark to be set to: ${expectedValue}`);
        activateBlur()
            .then(() => expect(pageObject.getOversightOptionRemark().getAttribute("value")).toEqual(expectedValue));

    }

    function verifyOversightOptionContent(presentNames: string[], unpresentNames: string[]) {
        presentNames.forEach(name => {
            console.log(`Expecting oversight option to be present:${name}`);
            expect(pageObject.getOversightOptionRow(name).isPresent()).toBeTruthy();
        });
        unpresentNames.forEach(name => {
            console.log(`Expecting oversight option NOT to be present:${name}`);
            expect(pageObject.getOversightOptionRow(name).isPresent()).toBeFalsy();
        });
    }

    function verifyOversightCompletedRemark(expectedValue: string) {
        console.log(`Expecting OversightCompletedRemark value: ${expectedValue}`);
        activateBlur()
            .then(() => expect(pageObject.getOversightCompletedRemark().getAttribute("value")).toEqual(expectedValue));

    }

    function activateBlur() {
        console.log(`Making sure blur is triggered`);
        return pageObject.getDpaMainNameHeader().click();
    }

    function verifyOversightDateAndRemarkRemoved() {
        console.log(`Expecting OversightDateAndRemarkRemoved`);
        expect(pageObject.getOversightDateRow().isPresent()).toBeFalsy();
    }

}); 