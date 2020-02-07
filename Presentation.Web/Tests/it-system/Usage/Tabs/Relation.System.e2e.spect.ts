import login = require("../../../Helpers/LoginHelper");
import InterfaceHelper = require("../../../Helpers/InterfaceCatalogHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../../Helpers/SystemCatalogHelper");
import SystemUsageHelper = require("../../../Helpers/SystemUsageHelper");
import LocalItSystemNavigation = require("../../../Helpers/SideNavigation/LocalItSystemNavigation");
import ItSystemNavigation = require("../../../Helpers/SideNavigation/ItSystemNavigation");

describe("User is able to create and view relation",
    () => {
        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper();
        var ec = protractor.ExpectedConditions;

        beforeAll(() => {
            loginHelper.loginAsGlobalAdmin();
            testFixture.longRunningSetup();
        });

        beforeEach(() => {
            testFixture.cleanupState();
            testFixture.enableLongRunningTest();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        it("User can see current relations ",
            () => {

            });

    });

