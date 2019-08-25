import Login = require("../../Helpers/LoginHelper");
import CatalogHelper = require("../../Helpers/CatalogHelper");
import ItSystemEditPo = require("../../PageObjects/it-system/Catalog/ItSystemCatalog.po")
import Constants = require("../../Utility/Constants");
import WaitTimers = require("../../Utility/WaitTimers");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");

describe("ITSystem Catalog accessibility tests", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemEditPo();
    var consts = new Constants();
    var waitUpTo = new WaitTimers();
    var testFixture = new TestFixtureWrapper();
    var findCatalogColumnsFor = CatalogHelper.findCatalogColumnsFor;

    afterEach(() => {
        testFixture.cleanupState();
    });

    beforeAll(() => {
        testFixture.enableLongRunningTest();
        loginHelper.loginAsLocalAdmin();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("Local Admin cannot create items in It-system catalog", () => {
        loginHelper.loginAsLocalAdmin()
            .then(() => {
                return CatalogHelper.isCreateButtonVisible(false);
            });
    });

    it("Regular user cannot create items in IT-system catalog", () => {
        loginHelper.loginAsRegularUser()
            .then(() => {
                return pageObject.getPage();
            })
            .then(() => {
                return browser.wait(pageObject.waitForKendoGrid(), waitUpTo.twentySeconds);;
            })
            .then(() => {
                return CatalogHelper.isCreateButtonVisible(false);;
            });
    });

    it("Global Admin can create and delete It-system catalog", () => {
        const catalogName = "catalog" + new Date().getTime();
        loginHelper.loginAsGlobalAdmin()
            .then(() => {
                return loadPage();
            })
            .then(() => {
                return waitForKendoGrid();
            })
            .then(() => {
                console.log("Making sure " + catalogName + " does not exist");
                return expect(findCatalogColumnsFor(catalogName)).toBeEmptyArray();
            })
            .then(() => {
                console.log("Creating catalog");
                return CatalogHelper.createCatalog(catalogName);
            })
            .then(() => {
                console.log("Loading page after catalog creation");
                return loadPage();
            })
            .then(() => {
                return waitForKendoGrid();
            })
            .then(() => {
                return expect(findCatalogColumnsFor(catalogName).first().getText()).toEqual(catalogName);
            })
            .then(() => {
                console.log("Deleting catalog");
                return CatalogHelper.deleteCatalog(catalogName);
            })
            .then(() => {
                console.log("Verify that catalog is deleted");
                return loadPage();
            })
            .then(() => {
                return waitForKendoGrid();
            })
            .then(() => {
                return expect(findCatalogColumnsFor(catalogName)).toBeEmptyArray();
            });
    });

    function waitForKendoGrid() {
        return CatalogHelper.waitForKendoGrid();
    }

    function loadPage() {
        console.log("Loading catalog page");
        return pageObject.getPage();
    }
});




