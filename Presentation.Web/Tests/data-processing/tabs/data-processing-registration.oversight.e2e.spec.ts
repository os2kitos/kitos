"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingRegistrationOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-registration.overview.po");
import DataProcessingRegistrationEditOversightPageObject = require("../../PageObjects/Data-Processing/Tabs/data-processing-registration.edit.oversight.po");
import DataProcessingRegistrationHelper = require("../../Helpers/DataProcessingRegistrationHelper");
import Select2Helper = require("../../Helpers/Select2Helper");

describe("Data processing registration oversight detail tests", () => {

    const loginHelper = new Login();
    const pageObjectOverview = new DataProcessingRegistrationOverviewPageObject();
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
            var dropdownInterval = "Hver andet år";
            var intervalRemark = createRemark();

            dpaHelper.createAndOpenDataProcessingRegistration(name)
                .then(() => pageObject.getOversightPage())
                .then(() => pageObject.getOversightIntervalOptionRemark().sendKeys(intervalRemark))
                .then(() => dpaHelper.changeOversightInterval(dropdownInterval))
                .then(() => verifyOversightInterval(dropdownInterval))
                .then(() => verifyOversightIntervalNote(intervalRemark));
        });

    function verifyOversightInterval(selectedValue: string) {
        console.log(`Expecting oversight interval to be set to: ${selectedValue}`);
        expect(Select2Helper.getData("s2id_oversightInterval_config").getText()).toEqual(selectedValue);
    }

    function verifyOversightIntervalNote(expectedValue: string) {
        console.log(`Expecting oversight interval note to be set to: ${expectedValue}`);
        expect(pageObject.getOversightIntervalOptionRemark().getAttribute("value")).toEqual(expectedValue);
    }




});