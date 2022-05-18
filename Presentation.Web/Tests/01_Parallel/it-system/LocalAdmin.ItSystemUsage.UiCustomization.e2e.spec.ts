import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import SystemCatalogHelper = require("../../Helpers/SystemCatalogHelper");
import OrgHelper = require("../../Helpers/OrgHelper");
import SystemUsageHelper = require("../../Helpers/SystemUsageHelper");
import NavigationHelper = require("../../Utility/NavigationHelper");
import LocalItSystemNavigationSrefs = require("../../Helpers/SideNavigation/LocalItSystemNavigationSrefs");
import Select2Helper = require("../../Helpers/Select2Helper");

describe("Local admin is able customize the IT-System usage UI", () => {

    var loginHelper = new Login();
    var testFixture = new TestFixtureWrapper();
    var navigation = new NavigationHelper();

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("Disabling GDPR will hide the GDPR tab", () => {
        var systemName = createName("SystemName");
        var orgName = createName("OrgName");

        return loginHelper.loginAsGlobalAdmin()
            .then(() => SystemCatalogHelper.createSystem(systemName))
            .then(() => OrgHelper.createOrg(orgName))
            .then(() => OrgHelper.activateSystemForOrg(systemName, orgName))
            .then(() => navigation.getPage("/#/global-admin/local-admins"))
            .then(() => Select2Helper.select(orgName, "s2id_selectOrg"))
            .then(() => Select2Helper.select(loginHelper.getGlobalAdminCredentials().username, "selectUser"))
            .then(() => testTabCustomization(systemName, "ItSystemUsages.gdpr", LocalItSystemNavigationSrefs.GPDRSref));
    });

    function testTabCustomization(systemName: string, settingId: string, tabSref: string) {
        return verifyTabVisibility(systemName, tabSref, true)
            .then(() => toggleSetting(settingId))
            .then(() => verifyTabVisibility(systemName, tabSref, false));
    }

    function verifyTabVisibility(systemName: string, sref: string, expectedToBePresent: boolean) {
        return SystemUsageHelper.openLocalSystem(systemName)
            .then(() => expect(navigation.findSubMenuElement(sref).isPresent()).toBe(expectedToBePresent));
    }

    function toggleSetting(settingId: string) {
        return navigation.getPage("/#/local-config/system")
            .then(() => element(by.id("expand_collapse_ItSystemUsages")).click())
            .then(() => browser.waitForAngular())
            .then(() => element(by.id(settingId)).click())
            .then(() => browser.waitForAngular());
    }

    function createName(prefix: string) {
        return `${prefix}_${new Date().getTime()}`;
    }
});


