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
        loginHelper.loginAsLocalAdmin();
    });

    afterAll(() => {
        testFixture.cleanupState();
    });

    it("Can create catalog and delete it again", () => {
        const catalogName = "catalog" + new Date().getTime();

        console.log("Loading page");
        loadPage();
        waitForKendoGrid();

        console.log("Making sure " + catalogName + " does not exist");
        expect(findCatalogColumnsFor(catalogName)).toBeEmptyArray();

        console.log("Creating catalog");
        CatalogHelper.createCatalog(catalogName);

        console.log("Loading page after catalog creation");
        loadPage();
        waitForKendoGrid();
        expect(findCatalogColumnsFor(catalogName).first().getText()).toEqual(catalogName);

        console.log("Deleting catalog");
        CatalogHelper.deleteCatalog(catalogName);

        console.log("Verify that catalog is deleted");
        loadPage();
        expect(findCatalogColumnsFor(catalogName)).toBeEmptyArray();
    });

    function waitForKendoGrid() {
        return CatalogHelper.waitForKendoGrid();
    }

    function loadPage() {
        pageObject.getPage();
    }
});




