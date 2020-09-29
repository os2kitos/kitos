"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingRegistrationOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-registration.overview.po");
import DataProcessingRegistrationEditOversightPageObject = require("../../PageObjects/Data-Processing/Tabs/data-processing-registration.edit.oversight.po");
import DataProcessingRegistrationHelper = require("../../Helpers/DataProcessingRegistrationHelper");
import Select2Helper = require("../../Helpers/Select2Helper");

describe("Data processing agreement oversight detail tests", () => {

    const loginHelper = new Login();
    const pageObjectOverview = new DataProcessingRegistrationOverviewPageObject();
    const pageObject = new DataProcessingRegistrationEditOversightPageObject();
    const testFixture = new TestFixtureWrapper();
    const dpaHelper = DataProcessingRegistrationHelper;

    const createName = (index: number) => {
        return `Dpa${new Date().getTime()}_${index}`;
    }

    const createNote = () => {
        return `OversightNote_${new Date().getTime()}`;
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
            var name = createName(10);
            var dropdownInterval = "Hver andet år";
            var intervalNote = createNote();

            dpaHelper.createDataProcessingRegistration(name)
                .then(() => pageObjectOverview.findSpecificDpaInNameColumn(name))
                .then(() => dpaHelper.goToSpecificDataProcessingRegistration(name))
                .then(() => pageObject.getOversightPage())
                .then(() => pageObject.getOversightIntervalOptionNote().sendKeys(intervalNote))
                .then(() => dpaHelper.changeOversightInterval(dropdownInterval))
                .then(() => verifyOversightInterval(dropdownInterval))
                .then(() => verifyOversightIntervalNote(intervalNote));
        });

    function verifyOversightInterval(selectedValue: string) {
        console.log(`Expecting oversight interval to be set to: ${selectedValue}`);
        expect(Select2Helper.getData("s2id_oversightInterval").getText()).toEqual(selectedValue);
    }

    function verifyOversightIntervalNote(expectedValue: string) {
        console.log(`Expecting oversight interval note to be set to: ${expectedValue}`);
        expect(pageObject.getOversightIntervalOptionNote().getAttribute("value")).toEqual(expectedValue);
    }


});