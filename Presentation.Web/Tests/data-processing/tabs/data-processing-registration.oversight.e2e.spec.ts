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
            var name = createName();
            var Remark = createRemark();
            const dropdownInterval = "Hver andet år";
            const dropdownCompleted = "Ja";
            const date = DateHelper.getTodayAsString();

            dpaHelper.createAndOpenDataProcessingRegistration(name)
                .then(() => pageObject.getOversightPage())
                .then(() => expectOversightCompletedLatestDateVisibility(false))
                .then(() => pageObject.getOversightIntervalOptionRemark().sendKeys(Remark))
                .then(() => pageObject.getOversightCompletedRemark().sendKeys(Remark))
                .then(() => dpaHelper.changeOversightInterval(dropdownInterval))
                .then(() => dpaHelper.changeOversightCompleted(dropdownCompleted))
                .then(() => dpaHelper.changeOversightCompletedLatestDate(date))
                .then(() => activateBlur())
                .then(() => verifyOversightInterval(dropdownInterval))
                .then(() => verifyOversightIntervalRemark(Remark))
                .then(() => verifyOversightCompletedRemark(Remark))
                .then(() => verifyOversightCompleted(dropdownCompleted))
                .then(() => expectOversightCompletedLatestDateVisibility(true))
                .then(() => verifyOversightCompletedLatestDate(date));
        });

    function verifyOversightInterval(selectedValue: string) {
        console.log(`Expecting oversight interval to be set to: ${selectedValue}`);
        expect(Select2Helper.getData("s2id_oversightInterval_config").getText()).toEqual(selectedValue);
    }

    function verifyOversightIntervalRemark(expectedValue: string) {
        console.log(`Expecting oversight interval remark to be set to: ${expectedValue}`);
        expect(pageObject.getOversightIntervalOptionRemark().getAttribute("value")).toEqual(expectedValue);
    }

    function verifyOversightCompleted(selectedValue: string) {
        console.log(`Expecting oversight completed to be set to: ${selectedValue}`);
        expect(Select2Helper.getData("s2id_oversightCompleted_config").getText()).toEqual(selectedValue);
    }

    function verifyOversightCompletedRemark(expectedValue: string) {
        console.log(`Expecting oversight completed remark to be set to: ${expectedValue}`);
        expect(pageObject.getOversightCompletedRemark().getAttribute("value")).toEqual(expectedValue);
    }

    function verifyOversightCompletedLatestDate(expectedValue: string)
    {
        console.log(`Expecting completed latest date to be set to: ${expectedValue}`);
        expect(pageObject.getOversightCompletedLatestDate().getAttribute("value")).toEqual(expectedValue);
    }

    function expectOversightCompletedLatestDateVisibility(visible: boolean) {
        console.log(`Expecting visiblity of oversight completed date to be set to: ${visible}`);
        expect(pageObject.getOversightCompletedLatestDate().isPresent()).toBe(visible);
    }

    function activateBlur() {
        return pageObject.getOversightIntervalOptionRemark().click();
    }

}); 