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
    });

    beforeEach(() => {
        pageObject.getPage();
        browser.waitForAngular();
        
    });

    it("Create IT contract is clickable", () => {
        expect(headerButtons.createContract.isEnabled()).toBe(true);
    });

    it("Reset Filter is clickable", () => {
        expect(headerButtons.resetFilter.isEnabled()).toBe(true);
    });

    it("Save Filter is clickable", () => {
        expect(headerButtons.saveFilter.isEnabled()).toBe(true);
    });

    it("Use Filter is disabled", () => {
        expect(headerButtons.useFilter.getAttribute("disabled")).toEqual("true");
        expect(headerButtons.useFilter.isEnabled()).toBe(false);
    });

    it("Delete Filter is disabled", () => {
        expect(headerButtons.deleteFilter.getAttribute("disabled")).toEqual("true");
        expect(headerButtons.deleteFilter.isEnabled()).toBe(false);
    });

    it("User able to create contract", () => {

        var cName = "Contract 123";
   
        headerButtons.createContract.click();
        browser.waitForAngular();

        pageObject.contractName.sendKeys(cName);
        expect(pageObject.contractName.isDisplayed()).toBe(true);   

        pageObject.saveContractBtn.click();
        browser.waitForAngular();

        expect(columnObject.contractName.get(0).getText()).toEqual(cName);

    });


    it("User is able to delete Contract", () => {
        var cName = "Contract 123";

        expect(columnObject.contractName.get(0).getText()).toEqual(cName);

        columnObject.contractName.get(0).click();

        element(by.className("btn btn-sm ng-binding btn-danger")).click();

        browser.switchTo().alert().accept();

        expect(columnObject.contractName.count()).toEqual(0);


    });


});