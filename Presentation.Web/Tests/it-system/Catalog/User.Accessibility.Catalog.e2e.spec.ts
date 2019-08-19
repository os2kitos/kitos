import Login = require("../../Helpers/LoginHelper");
import CatalogHelper = require("../../Helpers/CatalogHelper");
import ItSystemEditPo = require("../../PageObjects/it-system/Catalog/ItSystemCatalog.po")
import Constants = require("../../Utility/Constants");
import WaitTimers = require("../../Utility/WaitTimers");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");

describe("LocalAdmin user tests", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemEditPo();
    var consts = new Constants();
    var waitUpTo = new WaitTimers();
    var testFixture = new TestFixtureWrapper();
    var ec = protractor.ExpectedConditions;

    beforeAll(() => {
    //    browser.driver.manage().deleteAllCookies();
    });

    afterAll(() => {
        testFixture.cleanupState();
    });

    it("Global Admin can create and delete It-system catalog", () => {
        loginHelper.loginAsGlobalAdmin();
        CatalogHelper.isCreateButtonVisible(true);
        createAndConfirm(consts.defaultCatalog);
        browser.driver.manage().deleteAllCookies();
    });

    it("Local Admin can create and delete It-system catalog", () => {
        loginHelper.loginAsLocalAdmin();
        CatalogHelper.isCreateButtonVisible(true);
        createAndConfirm(consts.defaultCatalog);
        browser.driver.manage().deleteAllCookies();
    });

    it("Regular user cannot create and delete It-system catalog", () => {
        loginHelper.loginAsRegularUser();
        pageObject.getPage();
        browser.wait(pageObject.waitForKendoGrid(), waitUpTo.twentySeconds);
        CatalogHelper.isCreateButtonVisible(false);
    });

    function createAndConfirm(name: string) {

        pageObject.getPage();
        browser.wait(pageObject.waitForKendoGrid(), waitUpTo.twentySeconds);
        expect(pageObject.kendoToolbarWrapper.getFilteredColumnElement(pageObject.kendoToolbarWrapper.columnObjects().catalogName, name)).toBeEmptyArray();

        CatalogHelper.createCatalog(name);

        pageObject.getPage();
        expect(pageObject.kendoToolbarWrapper.getFilteredColumnElement(pageObject.kendoToolbarWrapper.columnObjects().catalogName, name).first().getText()).toEqual(name);

        CatalogHelper.deleteCatalog(name);

        pageObject.getPage();
        browser.wait(pageObject.waitForKendoGrid(), waitUpTo.twentySeconds);
        expect(pageObject.kendoToolbarWrapper.getFilteredColumnElement(pageObject.kendoToolbarWrapper.columnObjects().catalogName,name)).toBeEmptyArray();
    }

});




