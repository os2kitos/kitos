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
            const remark = createRemark();
            const dropdownInterval = "Hver andet år";
            const dropdownCompleted = "Ja";
            const oversightIntervalId = "s2id_oversightInterval_config";
            const oversightCompletedId = "s2id_oversightCompleted_config";
            const date = DateHelper.getTodayAsString();

            dpaHelper.createAndOpenDataProcessingRegistration(name)
                .then(() => pageObject.getOversightPage())
                .then(() => expectOversightCompletedLatestDateVisibility(false))
                .then(() => pageObject.getOversightIntervalOptionRemark().sendKeys(remark))
                .then(() => pageObject.getOversightCompletedRemark().sendKeys(remark))
                .then(() => dpaHelper.changeOversightInterval(dropdownInterval))
                .then(() => dpaHelper.changeOversightCompleted(dropdownCompleted))
                .then(() => dpaHelper.changeOversightCompletedLatestDate(date))
                .then(() => activateBlur())
                .then(() => verifySelect2Value(dropdownInterval, oversightIntervalId))
                .then(() => VerifyAttributeValueIs(remark, pageObject.getOversightIntervalOptionRemark()))
                .then(() => VerifyAttributeValueIs(remark, pageObject.getOversightCompletedRemark()))
                .then(() => verifySelect2Value(dropdownCompleted, oversightCompletedId ))
                .then(() => expectOversightCompletedLatestDateVisibility(true))
                .then(() => VerifyAttributeValueIs(date, pageObject.getOversightCompletedLatestDate()));
        });

    function verifySelect2Value(selectedValue: string, selectedId: string) {
        console.log(`Expecting oversight completed to be set to: ${selectedValue}`);
        expect(Select2Helper.getData(selectedId).getText()).toEqual(selectedValue);
    }

    function expectOversightCompletedLatestDateVisibility(visible: boolean) {
        console.log(`Expecting visiblity of oversight completed date to be set to: ${visible}`);
        expect(pageObject.getOversightCompletedLatestDate().isPresent()).toBe(visible);
    }

    function VerifyAttributeValueIs(expectedValue: string, element: protractor.ElementFinder) {
        console.log(`Expecting completed latest date to be set to: ${expectedValue}`);
        expect(element.getAttribute("value")).toEqual(expectedValue);
    }

    function activateBlur() {
        return pageObject.getOversightIntervalOptionRemark().click();
    }

}); 