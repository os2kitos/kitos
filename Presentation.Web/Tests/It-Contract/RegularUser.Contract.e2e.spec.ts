import login = require("../Helpers/LoginHelper");
import ItSystemEditPo = require("../PageObjects/it-contract/ItContractOverview.po");

describe("Regular user tests", () => {

    var loginHelper = new login.Login();
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

    it("Create Contract button is disabled", () => {
        expect(pageObject.createItContract.getAttribute("disabled")).toBe("true");
    });


    it("Use filter + delete filter is disabled", () => {
        expect(headerButtons.deleteFilter.getAttribute("disabled")).toBe("true");
        expect(headerButtons.useFilter.getAttribute("disabled")).toBe("true");
    });


    it("Reset filter and Save filter is active", () => {
        expect(EC.elementToBeClickable(headerButtons.resetFilter));
        expect(EC.elementToBeClickable(headerButtons.saveFilter));
    });
});