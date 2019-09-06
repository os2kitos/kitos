import Login = require("../../Helpers/LoginHelper");
import CatalogHelper = require("../../Helpers/CatalogHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");

describe("Global Administrator is able to migrate from one system to another", () => {
    var loginHelper = new Login();
    var testFixture = new TestFixtureWrapper();
    var cataHelper = CatalogHelper;

    afterEach(() => {
        testFixture.cleanupState();
    });

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    }); 

    afterAll(() => {
        testFixture.disableLongRunningTest();
    });

    it("Global Administrator is able to see the move button", () => {
        loginHelper.loginAsGlobalAdmin();
        cataHelper.checkMigrationButtonVisibility(true);
    });

    it("Local admin is not able to see the move button", () => {
        loginHelper.loginAsLocalAdmin();
        cataHelper.checkMigrationButtonVisibility(false);
    });

    it("Regular user is not able to see the move button", () => {
        loginHelper.loginAsRegularUser();
        cataHelper.checkMigrationButtonVisibility(false);
    });
});