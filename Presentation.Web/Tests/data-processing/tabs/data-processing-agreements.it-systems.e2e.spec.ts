"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingAgreementOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../../Utility/WaitTimers");
import DataProcessingAgreementHelper = require("../../Helpers/DataProcessingAgreementHelper")
import SystemCatalogHelper = require("../../Helpers/SystemCatalogHelper");

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


    it("Assigning and removing systems",
        () => {
            const dpaName = createName(0);
            const system1 = createName(1);
            const system2 = createName(2);

            SystemCatalogHelper.createSystemAndActivateLocally(system1)
                .then(() => SystemCatalogHelper.createSystemAndActivateLocally(system2))
                .then(() => dpaHelper.createDataProcessingAgreement(dpaName))
                .then(() => dpaHelper.goToSpecificDataProcessingAgreement(dpaName))
                .then(() => dpaHelper.goToItSystems())
                .then(() => assignSystem(system1))
                .then(() => assignSystem(system2))
                .then(() => verifySystemContent([system1, system2]))
                .then(() => removeSystem(system1))
                .then(() => verifySystemContent([system2]));
        });

    function assignSystem(name: string) {

    }

    function verifySystemContent(names: string[]) {

    }

    function removeSystem(name: string) {

    }
});