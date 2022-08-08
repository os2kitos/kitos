import login = require("../../../../Helpers/LoginHelper");
import InterfaceHelper = require("../../../../Helpers/InterfaceCatalogHelper");
import TestFixtureWrapper = require("../../../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../../../Helpers/SystemCatalogHelper");
import SystemUsageHelper = require("../../../../Helpers/SystemUsageHelper");
import LocalItSystemNavigation = require("../../../../Helpers/SideNavigation/LocalItSystemNavigation");
import ItSystemNavigation = require("../../../../Helpers/SideNavigation/ItSystemNavigation");

describe("Regular user can",
    () => {
        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper();

        var itSystemWithInterfaceName = createName("SystemWithInterface");
        var itSystemName = createName("SystemWithoutInterface");
        var interfaceName = createName("Interface");

        beforeAll(() => {
            loginHelper.loginAsGlobalAdmin()
                .then(() => ItSystemHelper.createSystem(itSystemName))
                .then(() => ItSystemHelper.createSystem(itSystemWithInterfaceName))
                .then(() => ItSystemHelper.createLocalSystem(itSystemWithInterfaceName))
                .then(() => InterfaceHelper.createInterface(interfaceName))
                .then(() => InterfaceHelper.bindInterfaceToSystem(itSystemWithInterfaceName, interfaceName))
                .then(() => testFixture.cleanupState())
                .then(() => loginHelper.loginAsRegularUser())
                .then(() => console.log("Pre-test initialization finished"));
            },
            testFixture.longRunningSetup());

        beforeEach(() => {
            testFixture.enableLongRunningTest();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        it("View exposed interfaces from it system usage details",
            () => {
                SystemUsageHelper.openLocalSystem(itSystemWithInterfaceName)
                    .then(() => LocalItSystemNavigation.exposedInterfacesPage())
                    .then(() => console.log("Checking for interface"))
                    .then(() => browser.waitForAngular())
                    .then(() => expect(getLinkToInterface(interfaceName).isPresent()).toBeTruthy());
            });

        it("View exposed interfaces from it system details with interface",
            () => {
                ItSystemHelper.openSystem(itSystemWithInterfaceName)
                    .then(() => ItSystemNavigation.exposedInterfacesPage())
                    .then(() => console.log("Checking for interface"))
                    .then(() => browser.waitForAngular())
                    .then(() => expect(getLinkToInterface(interfaceName).isPresent()).toBeTruthy());
            });

        it("View exposed interfaces from it system details without interface",
            () => {
                ItSystemHelper.openSystem(itSystemName)
                    .then(() => ItSystemNavigation.exposedInterfacesPage())
                    .then(() => console.log("Checking for correct url"))
                    .then(() => expect(browser.getCurrentUrl()).toMatch(urlRegex()));
            });
    });

function createName(prefix: string) {
    return `${prefix}_Regular_user_can_${new Date().getTime()}`;
}

function getLinkToInterface(interfaceName: string) {
    return element(by.linkText(interfaceName));
}

function urlRegex() {
    return /\S*\/#\/system\/edit\/\d+\/interfaces/;
}