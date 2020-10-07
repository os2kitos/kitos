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
            const dropdownCompleted = "Ja";
            const date = DateHelper.getTodayAsString();

            // Creating and navigating to DPR
            dpaHelper.createAndOpenDataProcessingRegistration(name)
                .then(() => pageObject.getOversightPage())
                .then(() => pageObject.getOversightIntervalRemark().sendKeys(intervalRemark))
                .then(() => dpaHelper.changeOversightInterval(dropdownInterval))
                .then(() => activateBlur())
                .then(() => verifyOversightInterval(dropdownInterval))
                .then(() => verifyOversightIntervalRemark(intervalRemark))
                // Oversight options
                .then(() => dpaHelper.assignOversightOption(oversightOptionName))
                .then(() => verifyOversightOptionContent([oversightOptionName], []))
                .then(() => dpaHelper.removeOversightOption(oversightOptionName))
                .then(() => verifyOversightOptionContent([], [oversightOptionName]))
                // Oversight option remark
                .then(() => pageObject.getOversightOptionRemark().sendKeys(oversightOptionRemark))
                .then(() => activateBlur())
                .then(() => verifyOversightOptionRemark(intervalRemark))
                // Oversight is completed
                .then(() => expectOversightCompletedLatestDateVisibility(false))
                .then(() => dpaHelper.changeOversightCompleted(dropdownCompleted))
                .then(() => dpaHelper.changeOversightCompletedLatestDate(date))
                .then(() => pageObject.getOversightCompletedRemark().sendKeys(completedRemark))
                .then(() => activateBlur())
                .then(() => verifyOversightCompletedRemark(completedRemark))
                .then(() => verifyOversightCompleted(dropdownCompleted))
                .then(() => expectOversightCompletedLatestDateVisibility(true))
                .then(() => verifyOversightCompletedDate(date));
        });
    

    function verifyOversightCompleted(selectedValue: string) {
        console.log(`Expecting oversight completed to be set to: ${selectedValue}`);
        expect(Select2Helper.getData("s2id_oversightCompleted_config").getText()).toEqual(selectedValue);
    }

    function verifyOversightInterval(selectedValue: string) {
        console.log(`Expecting oversight completed to be set to: ${selectedValue}`);
        expect(Select2Helper.getData("s2id_oversightInterval_config").getText()).toEqual(selectedValue);
    }

    function verifyOversightIntervalRemark(expectedValue: string) {
        console.log(`Expecting oversight interval remark to be set to: ${expectedValue}`);
        expect(pageObject.getOversightIntervalRemark().getAttribute("value")).toEqual(expectedValue);
    }

    function expectOversightCompletedLatestDateVisibility(visible: boolean) {
        console.log(`Expecting visiblity of oversight completed date to be set to: ${visible}`);
        expect(pageObject.getLatestOversightCompletedDate().isPresent()).toBe(visible);
    }

    function verifyOversightOptionRemark(expectedValue: string) {
        console.log(`Expecting oversight option remark to be set to: ${expectedValue}`);
        expect(pageObject.getOversightOptionRemark().getAttribute("value")).toEqual(expectedValue);
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
        expect(pageObject.getOversightCompletedRemark().getAttribute("value")).toEqual(expectedValue);
    }

    function verifyOversightCompletedDate(expectedValue: string) {
        console.log(`Expecting OversightCompletedDate value: ${expectedValue}`);
        expect(pageObject.getLatestOversightCompletedDate().getAttribute("value")).toEqual(expectedValue);
    }

    function activateBlur() {
        return pageObject.getOversightIntervalRemark().click();

    }

}); 