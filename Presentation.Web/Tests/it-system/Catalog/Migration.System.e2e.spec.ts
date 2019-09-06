import Login = require("../../Helpers/LoginHelper");
import CatalogHelper = require("../../Helpers/CatalogHelper");
import CSSLocator = require("../../Object-wrappers/CSSLocatorHelper");
import ItSystemEditPo = require("../../PageObjects/it-system/Catalog/ItSystemCatalog.po")
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import Constants = require("../../Utility/Constants");

describe("Global Administrator is able to migrate from one system to another", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemEditPo();
    var testFixture = new TestFixtureWrapper();
    var cssHelper = new CSSLocator();
    var consts = new Constants();
    var findCatalogColumnsFor = CatalogHelper.findCatalogColumnsFor;

    afterEach(() => {
        testFixture.cleanupState();
    });

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    }); 

    afterAll(() => {
        testFixture.disableLongRunningTest();
    });

    it("Global Administrator is able to see the move button", () => {
        loginHelper.loginAsGlobalAdmin();
        pageObject.getPage();
        pageObject.waitForKendoGrid();
        pageObject.kendoToolbarWrapper.columnObjects().usedByName.first().click();
        expect(element(cssHelper.byDataElementType(consts.moveSystemButton)).isPresent()).toBe(true);
    });

    it("Local admin is not able to see the move button", () => {
        loginHelper.loginAsLocalAdmin();
        pageObject.getPage();
        pageObject.waitForKendoGrid();
        pageObject.kendoToolbarWrapper.columnObjects().usedByName.first().click();
        expect(element(cssHelper.byDataElementType(consts.moveSystemButton)).isPresent()).toBe(false);
    });

    it("Regular user is not able to see the move button", () => {
        loginHelper.loginAsRegularUser();
        pageObject.getPage();
        pageObject.waitForKendoGrid();
        pageObject.kendoToolbarWrapper.columnObjects().usedByName.first().click();
        expect(element(cssHelper.byDataElementType(consts.moveSystemButton)).isPresent()).toBe(false);
    });

    it("Global Administrator is able to see available system for migration", () => {
        loginHelper.loginAsGlobalAdmin();
        pageObject.getPage();
        pageObject.waitForKendoGrid();
        pageObject.kendoToolbarWrapper.columnObjects().usedByName.first().click();
        browser.sleep(10000);

    });


});