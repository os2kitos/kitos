"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingAgreementOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../../Utility/WaitTimers");
import DataProcessingAgreementHelper = require("../../Helpers/DataProcessingAgreementHelper")

describe("Data processing agreement it-systems test", () => {

    const loginHelper = new Login();
    const pageObjectOverview = new DataProcessingAgreementOverviewPageObject();
    const testFixture = new TestFixtureWrapper();
    const waitUpTo = new WaitTimers();
    const ec = protractor.ExpectedConditions;
    const dpaHelper = DataProcessingAgreementHelper;

    const createName = (index: number) => {
        return `Dpa${new Date().getTime()}_${index}`;
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


    it("Creating and renaming data processing agreement",
        () => {

        });
});