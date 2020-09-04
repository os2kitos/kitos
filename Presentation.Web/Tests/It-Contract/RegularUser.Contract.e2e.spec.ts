import Login = require("../Helpers/LoginHelper");
import ItContractOverview = require("../PageObjects/it-contract/ItContractOverview.po");
import Constants = require("../Utility/Constants");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");

describe("Regular user has access to features in the contract overview", () => {

    var loginHelper = new Login();
    var consts = new Constants();
    var pageObject = new ItContractOverview();
    var headerButtons = pageObject.kendoToolbarWrapper.headerButtons();
    var columnObject = pageObject.kendoToolbarWrapper.columnObjects();
    var testFixture = new TestFixtureWrapper();

    beforeAll(() => {
        loginHelper.loginAsRegularUser();
    });

    beforeEach(() => {
        pageObject.getPage()
            .then(() => pageObject.waitForKendoGrid());
    });

    afterAll(() => {
        testFixture.cleanupState();
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
        expect(columnObject.contractName.getText()).toContain(consts.defaultItContractName);
    });
});