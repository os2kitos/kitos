import Login = require("../../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import SystemCatalogHelper = require("../../../Helpers/SystemCatalogHelper");
import OrgHelper = require("../../../Helpers/OrgHelper");
import SystemUsageHelper = require("../../../Helpers/SystemUsageHelper");
import NavigationHelper = require("../../../Utility/NavigationHelper");
import LocalItSystemNavigationSrefs = require("../../../Helpers/SideNavigation/LocalItSystemNavigationSrefs");
import Select2Helper = require("../../../Helpers/Select2Helper");
import Constants = require("../../../Utility/Constants");

describe("Local admin is able customize the IT-System usage UI", () => {

    var loginHelper = new Login();
    var testFixture = new TestFixtureWrapper();
    var navigation = new NavigationHelper();
    var consts = new Constants();

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    var localSystemPath: string | null = null;

    it("Disabling Tabs/fields will hide the tabs/fields on the IT-System details page", () => {
        var systemName = createName("SystemName");
        var orgName = createName("OrgName");

        return loginHelper.loginAsGlobalAdmin()
            .then(() => SystemCatalogHelper.createSystem(systemName))
            .then(() => OrgHelper.createOrg(orgName))
            .then(() => OrgHelper.activateSystemForOrg(systemName, orgName))
            .then(() => navigation.getPage("/#/global-admin/local-admins"))
            .then(() => Select2Helper.select(orgName, "s2id_selectOrg"))
            .then(() => Select2Helper.select(loginHelper.getGlobalAdminCredentials().username, "selectUser"))
            .then(() => Select2Helper.select(loginHelper.getGlobalAdminCredentials().username, "selectUser"))
            .then(() => verifyRiskAssessmentFieldVisibility(systemName))
            .then(() => testTabCustomization(systemName, "ItSystemUsages.gdpr", LocalItSystemNavigationSrefs.GPDRSref))
            .then(() => testTabCustomization(systemName, "ItSystemUsages.advice", LocalItSystemNavigationSrefs.adviceSref))
            .then(() => testTabCustomization(systemName, "ItSystemUsages.archiving", LocalItSystemNavigationSrefs.archivingSref))
            .then(() => testTabCustomization(systemName, "ItSystemUsages.interfaces", LocalItSystemNavigationSrefs.exposedInterfacesSref))
            .then(() => testTabCustomization(systemName, "ItSystemUsages.systemRelations", LocalItSystemNavigationSrefs.relationsSref))
            .then(() => testTabCustomization(systemName, "ItSystemUsages.organization", LocalItSystemNavigationSrefs.organizationSref))
            .then(() => testTabCustomization(systemName, "ItSystemUsages.hierarchy", LocalItSystemNavigationSrefs.hierarchySref))
            .then(() => testTabCustomization(systemName, "ItSystemUsages.systemRoles", LocalItSystemNavigationSrefs.rolesSref))
            .then(() => testTabCustomization(systemName, "ItSystemUsages.localKle", LocalItSystemNavigationSrefs.kleSref))
            .then(() => testTabCustomization(systemName, "ItSystemUsages.localReferences", LocalItSystemNavigationSrefs.referencesSref))
            .then(() => testFieldCustomization(systemName, "ItSystemUsages.contracts.selectContractToDetermineIfItSystemIsActive", LocalItSystemNavigationSrefs.contractsSref, "selectMainContractSection"))
            .then(() => testFieldCustomization(systemName, "ItSystemUsages.frontPage.lifeCycleStatus", LocalItSystemNavigationSrefs.mainPageSref, "lifeCycleStatus"))
            .then(() => testFieldGroupCustomization(systemName, "ItSystemUsages.frontPage.usagePeriod", LocalItSystemNavigationSrefs.mainPageSref, ["agreement-concluded", "agreement-expiration"]));
    });

    function testTabCustomization(systemName: string, settingId: string, tabSref: string) {
        console.log("testTabCustomization for ", systemName, " and tabSref:", tabSref, " settingId:", settingId);
        return verifyTabVisibility(systemName, tabSref, true)               //Check that the tab is visible before the change
            .then(() => toggleSetting(settingId))                           //Toggle the setting
            .then(() => verifyTabVisibility(systemName, tabSref, false));   //Verify that the tab has now been hidden
    }

    function testFieldCustomization(systemName: string, settingId: string, tabSref: string, settingElementId: string) {
        console.log("testFieldCustomization for ", systemName, " and tabSref:", tabSref, " settingId:", settingId);
        return verifySettingVisibility(systemName, tabSref, [settingElementId], true)                //Check that the setting is visible before the change
            .then(() => toggleSetting(settingId))                                                   //Toggle the setting
            .then(() => verifySettingVisibility(systemName, tabSref, [settingElementId], false));     //Verify that the setting has now been hidden
    }

    function testFieldGroupCustomization(systemName: string, settingId: string, tabSref: string, settingElementIds: Array<string>) {
        console.log("testFieldCustomization for ", systemName, " and tabSref:", tabSref, " affecting settings with ids:", settingElementIds.join(", "));
        return verifySettingVisibility(systemName, tabSref, settingElementIds, true)                //Check that the setting is visible before the change
            .then(() => toggleSetting(settingId))                                                   //Toggle the setting
            .then(() => verifySettingVisibility(systemName, tabSref, settingElementIds, false));     //Verify that the setting has now been hidden
    }

    function navigateToSystemUsage(systemName: string) {
        let navigationPromise;

        if (localSystemPath === null) {
            navigationPromise = SystemUsageHelper
                .openLocalSystem(systemName)
                .then(() => browser.getCurrentUrl())
                .then(url => localSystemPath = url.substr(browser.params.baseUrl.length));
        } else {
            // Save some time going directly to the system in stead of going through kendo
            navigationPromise = navigation.getPage(localSystemPath);
        }
        return navigationPromise;
    }

    function verifyTabVisibility(systemName: string, tabSref: string, expectedToBePresent: boolean) {
        console.log("verifyTabVisibility for ", systemName, " and tabSref:", tabSref, " expectedPresence:", expectedToBePresent);

        return navigateToSystemUsage(systemName)
            .then(() => expect(navigation.findSubMenuElement(tabSref).isPresent()).toBe(expectedToBePresent, `Failed to validate tab:${tabSref} to be ${expectedToBePresent ? "_present_" : "_removed_"}`));
    }

    function verifySettingVisibility(systemName: string, tabSref: string, settingElementIds: Array<string>, expectedToBePresent: boolean) {
        console.log("verifySettingVisibility for ", systemName, " and fields ", settingElementIds.join(", "), " located on tabSref:", tabSref, " expectedPresence:", expectedToBePresent);

        return navigateToSystemUsage(systemName)
            .then(() => expect(navigation.findSubMenuElement(tabSref).isPresent()).toBe(true, `Tab ${tabSref} is not present`))
            .then(() => navigation.findSubMenuElement(tabSref).click())
            .then(() => browser.waitForAngular())
            .then(() => {
                for (let settingElementId of settingElementIds) {
                    expect(element(by.id(settingElementId)).isPresent()).toBe(expectedToBePresent, `Setting: ${settingElementId} failed to meet expected visibility of ${expectedToBePresent}`);
                }
            });
    }

    function toggleSetting(settingId: string) {
        console.log("toggleSetting for ", settingId);
        return navigation.getPage("/#/local-config/system")
            .then(() => element(by.id("expand_collapse_ItSystemUsages")).click())
            .then(() => browser.waitForAngular())
            .then(() => element(by.id(settingId)).click())
            .then(() => browser.waitForAngular());
    }

    function verifyRiskAssessmentFieldVisibility(systemName: string) {
        console.log(`Testing gdpr.riskAssessment visibility for system: ${systemName}`);
        return navigateToSystemUsage(systemName)
            .then(() => navigation.findSubMenuElement(LocalItSystemNavigationSrefs.GPDRSref).click())
            .then(() => Select2Helper.selectWithNoSearch("Ja", consts.gdprRiskAssessmentSelect2Id))
            .then(() => testFieldCustomization(systemName, "ItSystemUsages.gdpr.plannedRiskAssessmentDate", LocalItSystemNavigationSrefs.GPDRSref, "plannedRiskAssessmentDate"));
    }

    function createName(prefix: string) {
        return `${prefix}_${new Date().getTime()}`;
    }
});


