import login = require("../Helpers/LoginHelper");
import ItSystemEditPo = require("../PageObjects/it-system/ItSystemOverview.po");

describe("Regular user tests", () => {
    var EC = protractor.ExpectedConditions;
    var loginHelper = new login();
    var pageObject = new ItSystemEditPo(); 
    var headerButtons = pageObject.kendoToolbarWrapper.headerButtons();
    var headerButtonsHelper = pageObject.kendoToolbarHelper.headerButtons;

    beforeAll(() => {
        loginHelper.loginAsRegularUser();
    });

    beforeEach(() => {
        pageObject.getPage();
        browser.waitForAngular();
    });

    it("Apply and delete filter buttons are disabled", () => {
        browser.wait(EC.visibilityOf(headerButtons.useFilter));
        expect(headerButtonsHelper.isUseDisabled()).toEqual("true");
        expect(headerButtonsHelper.isDeleteDisabled()).toEqual("true");
    });

});