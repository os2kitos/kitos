import login = require("../Helpers/LoginHelper");
import ItSystemEditPo = require("../PageObjects/it-system/ItSystemOverview.po");

describe("Regular user IT Systems tests", () => {
    var ec = protractor.ExpectedConditions;
    var loginHelper = new login();
    var pageObject = new ItSystemEditPo(); 
    var headerButtons = pageObject.kendoToolbarWrapper.headerButtons();
    var headerButtonsHelper = pageObject.kendoToolbarHelper.headerButtons;
    var gridObjects = pageObject.kendoToolbarWrapper.columnObjects();

    beforeAll(() => {
        loginHelper.loginAsRegularUser();
    });

    beforeEach(() => {
        pageObject.getPage();
    });

    it("Apply and delete filter buttons are disabled", () => {
        browser.wait(ec.visibilityOf(headerButtons.useFilter), 10000);
        expect(headerButtonsHelper.isUseDisabled()).toEqual("true");
        expect(headerButtonsHelper.isDeleteDisabled()).toEqual("true");
    });

    it("Can open IT system",() => {
        browser.wait(ec.visibilityOf(gridObjects.systemName.first()), 10000);
        gridObjects.systemName.first().click();
        expect(browser.getCurrentUrl()).toContain("system/usage");
    });

});