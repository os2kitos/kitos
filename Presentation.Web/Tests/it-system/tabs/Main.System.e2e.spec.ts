import login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../Helpers/SystemCatalogHelper");

describe("Global Admin can",
    () => {
        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper();
        var itSystemName = `ItSystemMainTabTest${new Date().getTime()}`;

        beforeAll(() => {
            loginHelper.loginAsGlobalAdmin()
                .then(() => ItSystemHelper.createSystem(itSystemName))
                .then(() => testFixture.cleanupState());
        });

        beforeEach(() => {
            testFixture.enableLongRunningTest();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        it("Edit Recommended ArchiveDuty and add comment", () => {
            return loginHelper.loginAsGlobalAdmin()
                .then(() => ItSystemHelper.openSystem(itSystemName));
            //TODO: Test it
        });
    });
