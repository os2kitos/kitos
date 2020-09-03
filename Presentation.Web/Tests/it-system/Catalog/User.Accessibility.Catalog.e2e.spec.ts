import Login = require("../../Helpers/LoginHelper");
import SystemCatalogHelper = require("../../Helpers/SystemCatalogHelper");
import ItSystemEditPo = require("../../PageObjects/it-system/Catalog/ItSystemCatalog.po")
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");

describe("ITSystem Catalog accessibility tests", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemEditPo();
    var testFixture = new TestFixtureWrapper();
    var findCatalogColumnsFor = SystemCatalogHelper.findCatalogColumnsFor;
    var until = protractor.ExpectedConditions;

    afterEach(() => {
        testFixture.cleanupState();
    });

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
    });

    it("Local Admin can still delete IT-system Catalogs that have been created locally", () => {
        const systemName = createSystemName();
        loginHelper.loginAsGlobalAdmin()
            .then(() => loadPage())
            .then(() => waitForKendoGrid())
            .then(() => expectCreateButtonVisibility(true))
            .then(() => expectNoSystemWithName(systemName))
            .then(() => SystemCatalogHelper.createSystem(systemName))
            .then(() => {
                console.log("Deleting cookies");
                testFixture.cleanupState();
            }).then(() => {
                console.log("Logging in as Local Admin");
                loginHelper.loginAsLocalAdmin();
            }).then(() => loadPage())
            .then(() => waitForKendoGrid())
            .then(() => expectSystemWithName(systemName))
            .then(() => SystemCatalogHelper.deleteSystem(systemName))
            .then(() => loadPage())
            .then(() => waitForKendoGrid())
            .then(() => {
                console.log("Checking that the catalog have been deleted");
                expectNoSystemWithName(systemName);
            });
    });

        it("Global Admin can create and delete It-system catalog", () => {
            const systemName = createSystemName();
             loginHelper.loginAsGlobalAdmin()
            .then(() => loadPage())
            .then(() => waitForKendoGrid())
            .then(() => expectCreateButtonVisibility(true))
            .then(() => expectNoSystemWithName(systemName))
            .then(() => SystemCatalogHelper.createSystem(systemName))
            .then(() => {
                console.log("Loading page after catalog creation");
                loadPage();
            })
            .then(() => waitForKendoGrid())
            .then(() => expectSystemWithName(systemName))
            .then(() => SystemCatalogHelper.deleteSystem(systemName))
            .then(() => {
                console.log("Verify that catalog is deleted");
                loadPage();
            })
            .then(() => waitForKendoGrid())
            .then(() => expectNoSystemWithName(systemName));
    });

    function expectCreateButtonVisibility(expectedEnabledState: boolean){
        console.log("Expecting createCatalog visibility to be:" + expectedEnabledState);
        return expect(pageObject.kendoToolbarWrapper.headerButtons().systemCatalogCreate.isEnabled()).toBe(expectedEnabledState);
    }

    function waitForKendoGrid() {
        return SystemCatalogHelper.waitForKendoGrid();
    }

    function loadPage() {
        console.log("Loading system catalog page");
        return pageObject.getPage();
    }

    function createSystemName() {
        return "System" + new Date().getTime();
    }

    function expectSystemWithName(name: string) {
        console.log("Making sure " + name + " does exist");
        browser.wait(until.textToBePresentInElement(findCatalogColumnsFor(name).first(), name));
        return expect(findCatalogColumnsFor(name).first().getText()).toEqual(name);
    }

    function expectNoSystemWithName(name: string) {
        console.log("Making sure " + name + " does not exist");
        return expect(findCatalogColumnsFor(name)).toBeEmptyArray();
    }
});