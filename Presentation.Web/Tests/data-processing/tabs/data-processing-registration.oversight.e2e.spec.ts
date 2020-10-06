"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingRegistrationEditOversightPageObject = require("../../PageObjects/Data-Processing/Tabs/data-processing-registration.edit.oversight.po");
import DataProcessingRegistrationHelper = require("../../Helpers/DataProcessingRegistrationHelper");
import Select2Helper = require("../../Helpers/Select2Helper");

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
            var intervalRemark = createRemark();
            const dropdownInterval = "Hver andet år";
            const oversightOptionName = "Egen kontrol";
            const oversightOptionRemark = createRemark();

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
                .then(() => verifyOversightOptionRemark(intervalRemark));
        });

    function verifyOversightInterval(selectedValue: string) {
        console.log(`Expecting oversight interval to be set to: ${selectedValue}`);
        expect(Select2Helper.getData("s2id_oversightInterval_config").getText()).toEqual(selectedValue);
    }

    function verifyOversightIntervalRemark(expectedValue: string) {
        console.log(`Expecting oversight interval remark to be set to: ${expectedValue}`);
        expect(pageObject.getOversightIntervalRemark().getAttribute("value")).toEqual(expectedValue);
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

});