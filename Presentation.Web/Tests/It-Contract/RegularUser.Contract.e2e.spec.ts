import login = require("../Helpers/LoginHelper");
import ItSystemEditPo = require("../PageObjects/it-contract/ItContractOverview.po");

describe("Regular user tests", () => {

    var loginHelper = new login();
    var EC = protractor.ExpectedConditions;
    var pageObject = new ItSystemEditPo();
    var headerButtons = pageObject.kendoToolbarWrapper.headerButtons();
    var columnHeaders = pageObject.kendoToolbarWrapper.columnHeaders();
    var columnObject = pageObject.kendoToolbarWrapper.columnObjects();

    beforeAll(() => {
        loginHelper.loginAsRegularUser();
        browser.waitForAngular();
    });

    beforeEach(() => {
        pageObject.getPage();
        browser.waitForAngular();
    });

    it("Reset filter and Save filter is active", () => {
        expect(EC.elementToBeClickable(headerButtons.resetFilter));
        expect(EC.elementToBeClickable(headerButtons.saveFilter));
    });
});