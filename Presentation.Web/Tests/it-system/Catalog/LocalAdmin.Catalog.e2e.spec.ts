import Login = require("../../Helpers/LoginHelper");
import CatalogHelper = require("../../Helpers/CatalogHelper");
import ItSystemEditPo = require("../../PageObjects/it-system/Catalog/ItSystemCatalog.po")
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");

describe("LocalAdmin user tests", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemEditPo();
    var testFixture = new TestFixtureWrapper();
    var findCatalogColumnsFor = CatalogHelper.findCatalogColumnsFor;

    beforeAll(() => {
        testFixture.enableLongRunningTest();
        loginHelper.loginAsLocalAdmin();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("Can create catalog and delete it again", () => {
        const catalogName = "catalog" + new Date().getTime();

        loadPage()
            .then(() => {
                return  waitForKendoGrid();
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




