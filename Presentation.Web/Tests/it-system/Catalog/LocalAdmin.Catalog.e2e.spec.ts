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

    beforeAll(() => {
        loginHelper.loginAsLocalAdmin();
        browser.waitForAngular();
    });

    afterAll(() => {
        testFixture.cleanupState();
    });

    it("Can create catalog and delete it again", () => {
        pageObject.getPage();
        browser.wait(pageObject.waitForKendoGrid(), waitUpTo.twentySeconds);
        expect(pageObject.kendoToolbarWrapper.getFilteredColumnElement(pageObject.kendoToolbarWrapper.columnObjects()
            .catalogName,
            consts.defaultCatalog)).toBeEmptyArray();
        CatalogHelper.createCatalog(consts.defaultCatalog);
        pageObject.getPage();
        expect(pageObject.kendoToolbarWrapper.getFilteredColumnElement(pageObject.kendoToolbarWrapper.columnObjects()
            .catalogName,
            consts.defaultCatalog).first().getText()).toEqual(consts.defaultCatalog);
        CatalogHelper.deleteCatalog(consts.defaultCatalog);
        pageObject.getPage();
        browser.wait(pageObject.waitForKendoGrid(), waitUpTo.twentySeconds);
        expect(pageObject.kendoToolbarWrapper.getFilteredColumnElement(pageObject.kendoToolbarWrapper.columnObjects()
            .catalogName,
            consts.defaultCatalog)).toBeEmptyArray();
    });

});




