import Login = require("../../../Helpers/LoginHelper");
import SystemCatalogHelper = require("../../../Helpers/SystemCatalogHelper");
import ItSystemEditPo = require("../../../PageObjects/it-system/Catalog/ItSystemCatalog.po")
import OrganizationHelper = require("../../../Helpers/OrgHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");

describe("ITSystem Catalog main screen tests", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemEditPo();
    var testFixture = new TestFixtureWrapper();
    const orgHelper = OrganizationHelper;
    var findCatalogColumnsFor = SystemCatalogHelper.findCatalogColumnsFor;

    afterEach(() => {
        testFixture.cleanupState();
    });

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
    });

    it("Creating an organization with a special character and verifying if It System Licensee allows for a special character", () => {
        const orgName = createName("Licensee&Test");
        const systemName = createName("LicenseeTest");
        
        loginHelper.loginAsLocalAdmin()
            .then(() => orgHelper.createOrg(orgName))
            .then(() => loadPage())
            .then(() => waitForKendoGrid())
            .then(() => expectCreateButtonVisibility(true))
            .then(() => expectNoSystemWithName(systemName))
            .then(() => SystemCatalogHelper.createSystem(systemName))
            .then(() => SystemCatalogHelper.openSystem(systemName))
            .then(() => SystemCatalogHelper.assignLicensee(orgName));
    });

    function expectCreateButtonVisibility(expectedEnabledState: boolean) {
        console.log(`Expecting createCatalog visibility to be:${expectedEnabledState}`);
        return expect(pageObject.kendoToolbarWrapper.headerButtons().systemCatalogCreate.isEnabled()).toBe(expectedEnabledState);
    }

    function waitForKendoGrid() {
        return browser.waitForAngular().then(() => SystemCatalogHelper.waitForKendoGrid());
    }

    function loadPage() {
        console.log("Loading system catalog page");
        return pageObject.getPage();
    }

    function createName(prefix: string) {
        return `${prefix}_${new Date().getTime()}`;
    }

    function expectNoSystemWithName(name: string) {
        console.log(`Making sure ${name} does not exist`);
        return expect(findCatalogColumnsFor(name)).toBeEmptyArray();
    }
});