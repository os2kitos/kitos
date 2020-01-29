import login = require("../../../Helpers/LoginHelper");
import InterfaceHelper = require("../../../Helpers/InterfaceCatalogHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../../Helpers/SystemCatalogHelper");
import SystemUsageHelper = require("../../../Helpers/SystemUsageHelper");
import LocalItSystemNavigation = require("../../../Helpers/SideNavigation/LocalItSystemNavigation");
import ItSystemNavigation = require("../../../Helpers/SideNavigation/ItSystemNavigation");

describe("Regular user can",
    () => {
        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper();
        var itSystemName = createItSystemName();
        var interfaceName = createInterfaceName();
        var ec = protractor.ExpectedConditions;

        beforeAll(() => {
            loginHelper.loginAsGlobalAdmin()
                .then(() => ItSystemHelper.createSystem(itSystemName))
                .then(() => ItSystemHelper.createLocalSystem(itSystemName))
                .then(() => InterfaceHelper.createInterface(interfaceName))
                .then(() => InterfaceHelper.bindInterfaceToSystem(itSystemName, interfaceName))
                .then(() => browser.waitForAngular());
        }, testFixture.longRunningSetup());

        beforeEach(() => {
            testFixture.cleanupState();
            testFixture.enableLongRunningTest();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
            loginHelper.loginAsRegularUser()
                .then(() => ItSystemHelper.resetFilters())
                .then(() => browser.waitForAngular())
                .then(() => testFixture.cleanupState());
        });

        it("View exposed interfaces from it system usage details",
            () => {
                loginHelper.loginAsRegularUser()
                    .then(() => SystemUsageHelper.openLocalSystem(itSystemName))
                    .then(() => LocalItSystemNavigation.exposedInterfacesPage())
                    .then(() => console.log("Checking for interface"))
                    .then(() => browser.wait(ec.visibilityOf(getInterfaceName())))
                    .then(() => expect(getInterfaceName().getText()).toEqual(interfaceName));
            });

        it("View exposed interfaces from it system details",
            () => {
                loginHelper.loginAsRegularUser()
                    .then(() => ItSystemHelper.openSystem(itSystemName))
                    .then(() => ItSystemNavigation.exposedInterfacesPage())
                    .then(() => console.log("Checking for interface"))
                    .then(() => browser.wait(ec.visibilityOf(getInterfaceName())))
                    .then(() => expect(getInterfaceName().getText()).toEqual(interfaceName));
            });
    });

function createItSystemName() {
    return `ExposedInterface${new Date().getTime()}`;
}

function createInterfaceName() {
    return `Interface${new Date().getTime()}`;
}

function getInterfaceName() {
    return element(by.css("[data-ui-sref='it-system.interface-edit.interface-details({ id: exposure.id })']"));
}
