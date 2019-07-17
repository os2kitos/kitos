import login = require("../Helpers/LoginHelper");
import ItSystemEditPo = require("../PageObjects/it-contract/ItContractOverview.po");
import constants = require("../Utility/Constants");

describe("Regular user has access features in the contract overview", () => {

    var loginHelper = new login();
    var consts = new constants();
    var pageObject = new ItSystemEditPo();
    var headerButtons = pageObject.kendoToolbarWrapper.headerButtons();
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
        expect(headerButtons.useFilter.isEnabled()).toBe(false);
    });

    it("Delete Filter is disabled", () => {
        expect(headerButtons.deleteFilter.isEnabled()).toBe(false);
    });

    it("User can see contract ", () => {
        expect(columnObject.contractName.get(0).getText()).toEqual(consts.defaultItContractName);

    });





});