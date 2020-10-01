"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingRegistrationOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-registration.overview.po");
import DataProcessingRegistrationHelper = require("../../Helpers/DataProcessingRegistrationHelper")
import SystemCatalogHelper = require("../../Helpers/SystemCatalogHelper");

describe("Data processing registration it-systems test", () => {

    const loginHelper = new Login();
    const testFixture = new TestFixtureWrapper();
    const pageObject = new DataProcessingRegistrationOverviewPageObject();
    const dpaHelper = DataProcessingRegistrationHelper;

    const createName = (index: number) => {
        return `Dpa${new Date().getTime()}_${index}`;
    }

    const system1 = createName(1);
    const system2 = createName(2);

    beforeAll(() => {
        testFixture.enableLongRunningTest();
        loginHelper.loginAsGlobalAdmin()
            .then(() => SystemCatalogHelper.createSystemAndActivateLocally(system1))
            .then(() => SystemCatalogHelper.createSystemAndActivateLocally(system2))
            .then(() => testFixture.cleanupState())
            .then(() => loginHelper.loginAsLocalAdmin())
            .then(() => dpaHelper.checkAndEnableDpaModule());
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("Assigning and removing systems",
        () => {
            const dpaName = createName(3);

            dpaHelper.createDataProcessingRegistration(dpaName)
                .then(() => dpaHelper.goToSpecificDataProcessingRegistration(dpaName))
                .then(() => dpaHelper.goToItSystems())
                .then(() => dpaHelper.assignSystem(system1))
                .then(() => dpaHelper.assignSystem(system2))
                .then(() => verifySystemContent([system1, system2], []))
                .then(() => dpaHelper.removeSystem(system1))
                .then(() => verifySystemContent([system2], [system1]));
        });

    function verifySystemContent(presentSystems: string[], unpresentSystems: string[]) {
        presentSystems.forEach(name => {
            console.log(`Expecting system to be present:${name}`);
            expect(pageObject.getSystemRow(name).isPresent()).toBeTruthy();
        });
        unpresentSystems.forEach(name => {
            console.log(`Expecting system NOT to be present:${name}`);
            expect(pageObject.getSystemRow(name).isPresent()).toBeFalsy();
        });
    }
});