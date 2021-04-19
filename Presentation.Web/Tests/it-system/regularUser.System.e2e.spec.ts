import Login = require("../Helpers/LoginHelper");
import ItSystemOverviewPo = require("../PageObjects/it-system/Usage/ItSystemUsageOverview.po");
import WaitTimers = require("../Utility/waitTimers");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");

describe("Regular user IT Systems tests", () => {
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

    it("Apply and delete filter buttons are disabled", () => {
        loginHelper.loginAsRegularUser()
            .then(() => pageObject.getPage())
            .then(() => browser.wait(ec.presenceOf(headerButtons.useFilter), waitUpTo.twentySeconds))
            .then(() => expect(headerButtonsHelper.isUseDisabled()).toEqual("true"))
            .then(() => expect(headerButtonsHelper.isDeleteDisabled()).toEqual("true"));
    });

    it("Can open IT system",() => {
        loginHelper.loginAsRegularUser()
            .then(() => pageObject.getPage())
            .then(() => pageObject.waitForKendoGrid())
            .then(() => browser.wait(ec.presenceOf(gridObjects.systemName.first()), waitUpTo.twentySeconds))
            .then(() => gridObjects.systemName.first().click())
            .then(() => expect(browser.getCurrentUrl()).toContain("system/usage"));
    });
});