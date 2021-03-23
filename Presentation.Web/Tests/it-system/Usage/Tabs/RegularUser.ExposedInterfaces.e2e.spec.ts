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

        var itSystemWithInterfaceName = createItSystemInterfaceName();
        var itSystemName = createItSystemName();
        var interfaceName = createInterfaceName();
        var ec = protractor.ExpectedConditions;

        beforeAll(() => {
            loginHelper.loginAsGlobalAdmin()
                .then(() => ItSystemHelper.createSystem(itSystemName))
                .then(() => ItSystemHelper.createSystem(itSystemWithInterfaceName))
                .then(() => ItSystemHelper.createLocalSystem(itSystemWithInterfaceName))
                .then(() => InterfaceHelper.createInterface(interfaceName))
                .then(() => InterfaceHelper.bindInterfaceToSystem(itSystemWithInterfaceName, interfaceName))
                .then(() => testFixture.cleanupState())
                .then(() => loginHelper.loginAsRegularUser());
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
                    .then(() => browser.wait(ec.visibilityOf(getInterfaceName())))
                    .then(() => expect(getInterfaceName().getText()).toEqual(interfaceName));
            });

        it("View exposed interfaces from it system details with interface",
            () => {
                ItSystemHelper.openSystem(itSystemWithInterfaceName)
                    .then(() => ItSystemNavigation.exposedInterfacesPage())
                    .then(() => console.log("Checking for interface"))
                    .then(() => browser.wait(ec.visibilityOf(getInterfaceName())))
                    .then(() => expect(getInterfaceName().getText()).toEqual(interfaceName));
            });

        it("View exposed interfaces from it system details without interface",
            () => {
                ItSystemHelper.openSystem(itSystemName)
                    .then(() => ItSystemNavigation.exposedInterfacesPage())
                    .then(() => console.log("Checking for correct url"))
                    .then(() => expect(browser.getCurrentUrl()).toMatch(urlRegex()));
            });
    });

function createItSystemInterfaceName() {
    return `SystemWithInterface${new Date().getTime()}`;
}

function createItSystemName() {
    return `SystemWithoutInterface${new Date().getTime()}`;
}

function createInterfaceName() {
    return `Interface${new Date().getTime()}`;
}

function getInterfaceName() {
    return element(by.css("[data-ui-sref='it-system.interface-edit.main({ id: exposure.id })']"));
}

function urlRegex() {
    return /\S*\/#\/system\/edit\/\d+\/interfaces/;
}