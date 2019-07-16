import login = require("../Helpers/LoginHelper");
import ItSystemEditPo = require("../PageObjects/it-system/ItSystemOverview.po");
import waitTimers = require("../Utility/waitTimers");

describe("Regular user IT Systems tests", () => {
    var ec = protractor.ExpectedConditions;
    var loginHelper = new login();
    var pageObject = new ItSystemEditPo(); 
    var headerButtons = pageObject.kendoToolbarWrapper.headerButtons();
    var headerButtonsHelper = pageObject.kendoToolbarHelper.headerButtons;
    var gridObjects = pageObject.kendoToolbarWrapper.columnObjects();
    var waitUpTo = new waitTimers();

    beforeAll(() => {
        loginHelper.loginAsRegularUser();
    });

    beforeEach(() => {
        pageObject.getPage();
    });

    it("Apply and delete filter buttons are disabled", () => {
        browser.wait(ec.visibilityOf(headerButtons.useFilter), waitUpTo.twentySeconds);
        expect(headerButtonsHelper.isUseDisabled()).toEqual("true");
        expect(headerButtonsHelper.isDeleteDisabled()).toEqual("true");
    });

    it("Can open IT system",() => {
        browser.wait(ec.visibilityOf(gridObjects.systemName.first()), waitUpTo.twentySeconds);
        gridObjects.systemName.first().click();
        expect(browser.getCurrentUrl()).toContain("system/usage");
    });

});