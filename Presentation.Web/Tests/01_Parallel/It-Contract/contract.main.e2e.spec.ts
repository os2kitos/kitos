import Login = require("../../Helpers/LoginHelper");
import ContractHelper = require("../../Helpers/ContractHelper");
import ItSystemEditPo = require("../../PageObjects/it-system/Catalog/ItSystemCatalog.po")
import OrganizationHelper = require("../../Helpers/OrgHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");

describe("ItContract main screen tests", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemEditPo();
    var testFixture = new TestFixtureWrapper();
    const orgHelper = OrganizationHelper;
    var contractHelper = ContractHelper;

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

    it("Creating an organization with a special character and verifying if It Contract Supplier allows for a special character", () => {
        const testOrgName = createName("TestOrganization");
        const supplierOrgName = createName("Supplier&Test");
        const contractName = createName("SupplierTest");

        orgHelper.createOrg(supplierOrgName)
            .then(() => orgHelper.createOrg(testOrgName))
            .then(() => orgHelper.changeOrg(testOrgName))
            .then(() => loadPage())
            .then(() => contractHelper.createContractAndProceed(contractName))
            .then(() => contractHelper.assignSupplier(supplierOrgName))
            .then(() => contractHelper.validateSupplierHasCorrectValue(supplierOrgName));
    });
    
    function loadPage() {
        console.log("Loading system catalog page");
        return pageObject.getPage();
    }

    function createName(prefix: string) {
        return `${prefix}_${new Date().getTime()}`;
    }
});