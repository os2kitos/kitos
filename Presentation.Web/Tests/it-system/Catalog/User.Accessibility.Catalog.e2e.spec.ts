import Login = require("../../Helpers/LoginHelper");
import CatalogHelper = require("../../Helpers/SystemCatalogHelper");
import ItSystemEditPo = require("../../PageObjects/it-system/Catalog/ItSystemCatalog.po")
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");

describe("ITSystem Catalog accessibility tests", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemEditPo();
    var testFixture = new TestFixtureWrapper();
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

    it("Local Admin can still delete IT-system Catalogs that have been created locally", () => {
        const systemName = createSystemName();
        loginHelper.loginAsGlobalAdmin()
            .then(() => {
                return loadPage();
            }).then(() => {
                return waitForKendoGrid();
            }).then(() => {
                return expectCreateButtonVisibility(true);
            }).then(() => {
                return expectNoSystemWithName(systemName);
            }).then(() => {
                console.log("Creating system");
                return CatalogHelper.createSystem(systemName);
            }).then(() => {
                console.log("Deleting cookies");
                return testFixture.cleanupState();
            }).then(() => {
                console.log("Logging in as Local Admin");
                return loginHelper.loginAsLocalAdmin();
            }).then(() => {
                return loadPage();
            }).then(() => {
                return waitForKendoGrid();
            }).then(() => {
                return expectSystemWithName(systemName);
            }).then(() => {
                console.log("Deleting system");
                return CatalogHelper.deleteSystem(systemName);
            }).then(() => {
                return loadPage();
            }).then(() => {
                return waitForKendoGrid();
            }).then(() => {
                console.log("Checking that the catalog have been deleted");
                return expectNoSystemWithName(systemName);
            });
    });

        it("Global Admin can create and delete It-system catalog", () => {
            const systemName = createSystemName();
             loginHelper.loginAsGlobalAdmin()
            .then(() => {
                return loadPage();
            })
            .then(() => {
                return waitForKendoGrid();
            })
            .then(() => {
                return expectCreateButtonVisibility(true);
            })
            .then(() => {
                return expectNoSystemWithName(systemName);
            })
            .then(() => {
                console.log("Creating system");
                return CatalogHelper.createSystem(systemName);
            })
            .then(() => {
                console.log("Loading page after catalog creation");
                return loadPage();
            })
            .then(() => {
                return waitForKendoGrid();
            })
            .then(() => {
                return expectSystemWithName(systemName);
            })
            .then(() => {
                console.log("Deleting catalog");
                return CatalogHelper.deleteSystem(systemName);
            })
            .then(() => {
                console.log("Verify that catalog is deleted");
                return loadPage();
            })
            .then(() => {
                return waitForKendoGrid();
            })
            .then(() => {
                return expectNoSystemWithName(systemName);
            });
    });

    function expectCreateButtonVisibility(expectedEnabledState: boolean){
        console.log("Expecting createCatalog visibility to be:" + expectedEnabledState);
        return expect(pageObject.kendoToolbarWrapper.headerButtons().systemCatalogCreate.isEnabled()).toBe(expectedEnabledState);
    }

    function waitForKendoGrid() {
        return CatalogHelper.waitForKendoGrid();
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
        return expect(findCatalogColumnsFor(name).first().getText()).toEqual(name);
    }

    function expectNoSystemWithName(name: string) {
        console.log("Making sure " + name + " does not exist");
        return expect(findCatalogColumnsFor(name)).toBeEmptyArray();
    }
});