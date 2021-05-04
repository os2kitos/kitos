import Login = require("../Helpers/LoginHelper");
import ItSystemOverviewPo = require("../PageObjects/it-system/Usage/ItSystemUsageOverview.po");
import WaitTimers = require("../Utility/waitTimers");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");

describe("Local admin IT Systems tests", () => {
    var ec = protractor.ExpectedConditions;
    var loginHelper = new Login();
    var pageObject = new ItSystemOverviewPo();
    var waitUpTo = new WaitTimers();
    var testFixture = new TestFixtureWrapper();
    var headerButtons = pageObject.kendoToolbarWrapper.headerButtons();
    var headerButtonsHelper = pageObject.kendoToolbarHelper.headerButtons;
    var gridObjects = pageObject.kendoToolbarWrapper.columnObjects();
    var headerObjects = pageObject.kendoToolbarWrapper.columnHeaders();

    afterEach(() => {
        testFixture.cleanupState();
    });

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
    });

    it("Save filter to org button is enabled and visible for local admin" +
        "", () => {
        loginHelper.loginAsLocalAdmin()
            .then(() => pageObject.getPage())
            .then(() => expect(headerButtons.saveFilterToOrg.isDisplayed()).toBe(true));
        });

    it("Save filter to org button is not visible for other users" +
        "", () => {
        loginHelper.loginAsRegularUser()
                .then(() => pageObject.getPage())
            .then(() => expect(headerButtons.saveFilterToOrg.isDisplayed()).toBe(false));
        });

});