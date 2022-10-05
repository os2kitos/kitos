import Login = require("../../Helpers/LoginHelper");
import ItSystemOverviewPo = require("../../PageObjects/it-system/Usage/ItSystemUsageOverview.po");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");

describe("Local admin IT Systems tests", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemOverviewPo();
    var testFixture = new TestFixtureWrapper();
    var headerButtons = pageObject.kendoToolbarWrapper.headerButtons();

    afterEach(() => {
        testFixture.cleanupState();
    });

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
    });

    it("LocalAdmin dropdown is enabled and visible for local admin", () => {
        loginHelper.loginAsLocalAdmin()
            .then(() => pageObject.getPage())
            .then(() => pageObject.waitForKendoGrid())
            .then(() => expect(headerButtons.localAdminDropdownReference.isPresent()).toBe(true));
    });

    it("LocalAdmin dropdown is not visible for regular user", () => {
        loginHelper.loginAsRegularUser()
            .then(() => pageObject.getPage())
            .then(() => pageObject.waitForKendoGrid())
            .then(() => expect(headerButtons.localAdminDropdownReference.isPresent()).toBe(false));
    });

    it("LocalAdmin dropdown is not visible for global admin", () => {
        loginHelper.loginAsGlobalAdmin()
            .then(() => pageObject.getPage())
            .then(() => pageObject.waitForKendoGrid())
            .then(() => expect(headerButtons.localAdminDropdownReference.isPresent()).toBe(false));
    });
});