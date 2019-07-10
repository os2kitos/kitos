import login = require("../Helpers/LoginHelper");
import ItSystemEditPo = require("../PageObjects/it-system/ItSystemOverview.po");

describe("Regular user tests", () => {

    var loginHelper = new login();
    var pageObject = new ItSystemEditPo(); 
    var headerButtons = pageObject.kendoToolbarWrapper.headerButtons();
    var columnHeaders = pageObject.kendoToolbarWrapper.columnHeaders();
    var columnObject = pageObject.kendoToolbarWrapper.columnObjects();

    beforeAll(() => {
        loginHelper.loginAsRegularUser();
    });

    beforeEach(() => {
        pageObject.getPage();
        browser.waitForAngular();
    });

    it("Apply and delete filter buttons are disabled", () => {       
        expect(headerButtons.useFilter.getAttribute("disabled")).toEqual("true");
        expect(headerButtons.deleteFilter.getAttribute("disabled")).toEqual("true");
    });

});