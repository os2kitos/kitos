import login = require("../Helpers/LoginHelper");
import ItSystemEditPo = require("../PageObjects/it-system/ItSystemOverview.po");

describe("Regular user tests", () => {

    var loginHelper = new login.Login();
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

    it("Apply and delete filter buttons are disabled", () => {       
        expect(headerButtons.useFilter.getAttribute("disabled")).toEqual("true");
        expect(headerButtons.deleteFilter.getAttribute("disabled")).toEqual("true");
    });

    //it("IT systems can be sorted by name", () => {
    //    browser.wait(protractor.ExpectedConditions.visibilityOf(columnObject.systemName.first()));
    //    columnHeaders.systemName.click();
    //    browser.wait(protractor.ExpectedConditions.visibilityOf(columnObject.systemName.first()));
    //    var firstItemName = columnObject.systemName.first().getText();

    //    columnHeaders.systemName.click();
    //    browser.wait(protractor.ExpectedConditions.visibilityOf(columnObject.systemName.first()));

    //    expect(columnObject.systemName.last().getText()).toEqual(firstItemName);
    //});

    it("IT system can be opened", () => {

        columnObject.systemName.first().click();

        expect(browser.getCurrentUrl()).toContain("#/system/usage/");
    });


});