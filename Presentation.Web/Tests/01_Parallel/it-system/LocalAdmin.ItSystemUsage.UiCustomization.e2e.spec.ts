import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import SystemCatalogHelper = require("../../Helpers/SystemCatalogHelper");
import OrgHelper = require("../../Helpers/OrgHelper");
import SystemUsageHelper = require("../../Helpers/SystemUsageHelper");
import NavigationHelper = require("../../Utility/NavigationHelper");
import LocalItSystemNavigationSrefs1 = require("../../Helpers/SideNavigation/LocalItSystemNavigationSrefs");

describe("Local admin is able customize the IT-System usage UI", () => {

    var loginHelper = new Login();
    var testFixture = new TestFixtureWrapper();
    var navigation = new NavigationHelper();

    var systemName = createName("SystemName");
    var orgName = createName("OrgName");

    beforeAll(() => {
        testFixture.enableLongRunningTest();
        return loginHelper.loginAsGlobalAdmin()
            .then(() => SystemCatalogHelper.createSystem(systemName))
            .then(() => OrgHelper.createOrg(orgName))
            .then(() => OrgHelper.activateSystemForOrg(systemName, orgName))
        //TODO: Make global admin local admin in the new organization
            .then(() => loginHelper.logout())
            .then(() => loginHelper.loginAsLocalAdmin());
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("Disabling GDPR will hide the GDPR tab", () => {
        SystemUsageHelper.openLocalSystem(systemName)
            .then(() => expect(navigation.findSubMenuElement(LocalItSystemNavigationSrefs1.GPDRSref).isPresent()).toBe(true));

        //TODO: Extend
    });

    function createName(prefix: string) {
        return `${prefix}_${new Date().getTime()}`;
    }
});


