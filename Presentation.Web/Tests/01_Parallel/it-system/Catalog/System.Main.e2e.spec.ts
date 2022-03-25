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
        loginHelper.loginAsGlobalAdmin();
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
    });

    it("Creating an organization with a special character and verifying if It System Licensee allows for a special character", () => {
        const testOrgName = createName("TestOrganization");
        const licenseeOrgName = createName("Licensee&Test");
        const systemName = createName("LicenseeTest");
        
        orgHelper.createOrg(licenseeOrgName)
            .then(() => orgHelper.createOrg(testOrgName))
            .then(() => orgHelper.changeOrg(testOrgName))
            .then(() => console.log("Starting page loading"))
            .then(() => loadPage())
            .then(() => waitForKendoGrid())
            .then(() => console.log("Finished page loading"))
            //.then(() => SystemCatalogHelper.createSystem(systemName))
            .then(() => SystemCatalogHelper.openAnySystem())
            .then(() => SystemCatalogHelper.assignLicensee(licenseeOrgName))
            .then(() => SystemCatalogHelper.validateLicenseeHasCorrectValue(licenseeOrgName));
    });

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
});