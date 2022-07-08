"use strict";
import Login = require("../../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import DataProcessingRegistrationOverviewPageObject = require("../../../PageObjects/Data-Processing/data-processing-registration.overview.po");
import DataProcessingRegistrationHelper = require("../../../Helpers/DataProcessingRegistrationHelper")
import SystemCatalogHelper = require("../../../Helpers/SystemCatalogHelper");
import ItSystemUsageDataProcessing = require("../../../PageObjects/it-system/Usage/Tabs/ItSystemUsageDataProcessing.po");
import LocalItSystemNavigation = require("../../../Helpers/SideNavigation/LocalItSystemNavigation");

describe("Data processing registration it-systems test", () => {

    const loginHelper = new Login();
    const testFixture = new TestFixtureWrapper();
    const pageObject = new DataProcessingRegistrationOverviewPageObject();
    const dpaHelper = DataProcessingRegistrationHelper;

    const createName = (prefix: string) => {
        return `${prefix}_${new Date().getTime()}`;
    }

    const system1 = createName("System1");
    const system2 = createName("System2");

    beforeAll(() => {
        testFixture.enableLongRunningTest();
        loginHelper.loginAsGlobalAdmin()
            .then(() => SystemCatalogHelper.createSystemAndActivateLocally(system1))
            .then(() => SystemCatalogHelper.createSystemAndActivateLocally(system2))
            .then(() => testFixture.cleanupState())
            .then(() => loginHelper.loginAsLocalAdmin())
            .then(() => dpaHelper.checkAndEnableDpaModule());
    }, testFixture.longRunningSetup());

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("Assigning and removing systems",
        () => {
            const dprName = createName("DPR");

            dpaHelper.createAndOpenDataProcessingRegistration(dprName)
                .then(() => dpaHelper.goToItSystems())
                .then(() => dpaHelper.assignSystem(system1))
                .then(() => dpaHelper.assignSystem(system2))
                .then(() => verifyDPRContent([system1, system2], []))
                .then(() => verifyDPRIsPresentOnItSystemGDPRPage(system1, dprName))
                .then(() => dpaHelper.removeSystem(system1))
                .then(() => verifyDPRContent([system2], [system1]));
        });

    function verifyDPRContent(presentSystems: string[], unpresentSystems: string[]) {
        presentSystems.forEach(name => {
            console.log(`Expecting system to be present:${name}`);
            expect(pageObject.getSystemRow(name).isPresent()).toBeTruthy();
        });
        unpresentSystems.forEach(name => {
            console.log(`Expecting system NOT to be present:${name}`);
            expect(pageObject.getSystemRow(name).isPresent()).toBeFalsy();
        });
    }

    function verifyDPRIsPresentOnItSystemGDPRPage(systemName: string, dprName: string) {
        console.log(`Expecting system ${systemName} contain reference to DPR:${dprName}`);
        return dpaHelper.clickSystem(systemName)
            .then(() => LocalItSystemNavigation.openDataProcessingPage())
            .then(() => expect(ItSystemUsageDataProcessing.getDataProcessingLink(dprName).isPresent()).toBeTruthy())
            .then(() => ItSystemUsageDataProcessing.getDataProcessingLink(dprName).click())
            .then(() => browser.waitForAngular())
            .then(() => dpaHelper.goToItSystems());
    }
});