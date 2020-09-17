"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingAgreementOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../../Utility/WaitTimers");
import DataProcessingAgreementEditMainPageObject = require("../../PageObjects/Data-Processing/Tabs/data-processing-agreement.edit.main");
import DataProcessingAgreementHelper = require("../../Helpers/DataProcessingAgreementHelper")

describe("Data processing agreement main detail tests", () => {

    const loginHelper = new Login();
    const pageObjectOverview = new DataProcessingAgreementOverviewPageObject();
    const pageObject = new DataProcessingAgreementEditMainPageObject();
    const testFixture = new TestFixtureWrapper();
    const waitUpTo = new WaitTimers();
    const ec = protractor.ExpectedConditions;
    const dpaHelper = DataProcessingAgreementHelper;

    const createName = () => {
        return `Dpa${new Date().getTime()}`;
    }

    beforeAll(() => {
        loginHelper.loginAsGlobalAdmin();
        testFixture.enableLongRunningTest();
        dpaHelper.checkAndEnableDpaModule();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });


    it("Creating and renaming data processing agreement",
        () => {
            var name = createName();

            dpaHelper.createDataProcessingAgreement(name)
                .then(() => pageObjectOverview.findSpecificDpaInNameColumn(name))
                .then(() => dpaHelper.goToSpecificDataProcessingAgreement(name))
                .then(() => dpaHelper.goToRoles())
                .then(() => dpaHelper.assignRole("role", "local-global-admin-user@kitos.dk"));
        });



});