import Login = require("../../Helpers/LoginHelper");
import CatalogHelper = require("../../Helpers/CatalogHelper");
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

    it("Local Admin cannot create items in It-system catalog", () => {
        loginHelper.loginAsLocalAdmin()
            .then(() => {
                return loadPage();
            })
            .then(() => {
                return waitForKendoGrid();
            })
            .then(() => {
                return expectCreateButtonVisibility(false);
            });
    });

    it("Regular user cannot create items in IT-system catalog", () => {
        loginHelper.loginAsRegularUser()
            .then(() => {
                return loadPage();
            })
            .then(() => {
                return waitForKendoGrid();
            })
            .then(() => {
                return expectCreateButtonVisibility(false);
            });
    });

    it("Local Admin can still delete IT-system Catalogs that have been created locally", () => {
        const catalogName = createCatalogName();
        loginHelper.loginAsGlobalAdmin()
            .then(() => {
                return loadPage();
            }).then(() => {
                return waitForKendoGrid();
            }).then(() => {
                return expectCreateButtonVisibility(true);
            }).then(() => {
                return expectNoCatalogWithName(catalogName);
            }).then(() => {
                console.log("Creating catalog");
                return CatalogHelper.createCatalog(catalogName);
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
                return expectCatalogWithName(catalogName);
            }).then(() => {
                console.log("Deleting catalog");
                return CatalogHelper.deleteCatalog(catalogName);
            }).then(() => {
                return loadPage();
            }).then(() => {
                return waitForKendoGrid();
            }).then(() => {
                console.log("Checking that the catalog have been deleted");
                return expectNoCatalogWithName(catalogName);
            });
    });

        it("Global Admin can create and delete It-system catalog", () => {
            const catalogName = createCatalogName();
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
                return expectNoCatalogWithName(catalogName);
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
                return expectCatalogWithName(catalogName);
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
                return expectNoCatalogWithName(catalogName);
            });
    });

    function expectCreateButtonVisibility(expectedEnabledState: boolean)
    {
        console.log("Expecting createCatalog visibility to be:" + expectedEnabledState);
        return expect(pageObject.kendoToolbarWrapper.headerButtons().systemCatalogCreate.isEnabled()).toBe(expectedEnabledState);
    }

    function waitForKendoGrid()
    {
        return CatalogHelper.waitForKendoGrid();
    }

    function loadPage() {
        console.log("Loading catalog page");
        return pageObject.getPage();
    }

    function createCatalogName()
    {
        return "Catalog" + new Date().getTime();
    }

    function expectCatalogWithName(name: string)
    {
        console.log("Making sure " + name + " does exist");
        return expect(findCatalogColumnsFor(name).first().getText()).toEqual(name);
    }

    function expectNoCatalogWithName(name: string)
    {
        console.log("Making sure " + name + " does not exist");
        return expect(findCatalogColumnsFor(name)).toBeEmptyArray();
    }
});




