import NavigationHelper = require("../Utility/NavigationHelper");
import OrgHelper = require("./OrgHelper");
import Select2Helper = require("./Select2Helper");
import Login = require("./LoginHelper");

export interface UiCustomizationTestConfig {
    module: string
    localAdminArea: string
    createEntity: (name: string) => webdriver.promise.Promise<unknown>
    navigateToEntity: (name: string) => webdriver.promise.Promise<unknown>
}

export class UiCustomizationTestHelper {

    knownEntityPath: string | null = null;
    entityName = "";
    orgName = "";
    private readonly navigation = new NavigationHelper();
    private readonly loginHelper = new Login();
    constructor(private readonly config: UiCustomizationTestConfig) {
        this.entityName = this.createName("entity");
        this.orgName = this.createName("org");
        this.knownEntityPath = null;
    }

    setupUserAndOrg() {
        return this.loginHelper.loginAsGlobalAdmin()
            .then(() => OrgHelper.createOrg(this.orgName))
            .then(() => OrgHelper.changeOrg(this.orgName))
            .then(() => this.config.createEntity(this.entityName)
                .then(() => this.navigation.getPage("/#/global-admin/local-admins"))
                .then(() => Select2Helper.select(this.orgName, "s2id_selectOrg"))
                .then(() => Select2Helper.select(this.loginHelper.getGlobalAdminCredentials().username, "selectUser")));
    }

    testTabCustomization(settingId: string, tabSref: string) {
        console.log("testTabCustomization for ", this.entityName, " and tabSref:", tabSref, " settingId:", settingId);
        return this.verifyTabVisibility(this.entityName, tabSref, true)               //Check that the tab is visible before the change
            .then(() => this.toggleSetting(settingId))                           //Toggle the setting
            .then(() => this.verifyTabVisibility(this.entityName, tabSref, false));   //Verify that the tab has now been hidden
    }

    testFieldCustomization(settingId: string, tabSref: string, settingElementId: string) {
        return this.testFieldGroupCustomization(settingId, tabSref, [settingElementId]);
    }

    testFieldGroupCustomization(settingId: string, tabSref: string, settingElementIds: Array<string>) {
        console.log("testFieldCustomization for ", this.entityName, " and tabSref:", tabSref, " affecting settings with ids:", settingElementIds.join(", "));
        return this.verifySettingVisibility(this.entityName, tabSref, settingElementIds, true)                //Check that the setting is visible before the change
            .then(() => this.toggleSetting(settingId))                                                   //Toggle the setting
            .then(() => this.verifySettingVisibility(this.entityName, tabSref, settingElementIds, false));     //Verify that the setting has now been hidden
    }

    testFieldCustomizationWithSubtreeIsComplete(settingId: string, tabSref: string, settingElementId: string) {
        return this.testFieldGroupCustomizationWithSubtreeIsComplete(settingId, tabSref, [settingElementId]);
    }

    testFieldGroupCustomizationWithSubtreeIsComplete(settingId: string, tabSref: string, settingElementIds: Array<string>) {
        console.log("testFieldCustomization for ", this.entityName, " and tabSref:", tabSref, " affecting settings with ids:", settingElementIds.join(", "));
        return this.verifySettingVisibility(this.entityName, tabSref, settingElementIds, true)                //Check that the setting is visible before the change
            .then(() => this.toggleSetting(settingId))                                                     //Toggle the setting
            .then(() => this.verifySettingVisibility(this.entityName, tabSref, settingElementIds, false))     //Verify that the setting has now been hidden
            .then(() => this.toggleSetting(settingId));                                                    //Toggle the setting
    }

    testTabCustomizationWithSubtreeIsComplete(settingIds: Array<string>, tabSref: string) {
        console.log("testTabCustomization for ", this.entityName, " and tabSref:", tabSref);

        return this.verifyTabVisibility(this.entityName, tabSref, true)                                                                   //Check that the tab is visible before the change
            .then(() => this.openUiConfiguration())                                                                            //Open Ui Customization page and show details
            .then(() => settingIds.reduce((promise, item) => {
                return promise.then(() => this.checkStateAndToggleSetting(item, false));
            }, protractor.promise.when([])))                                                                              //Toggle the setting
            .then(() => this.verifyTabVisibility(this.entityName, tabSref, false));                                                       //Verify that the tab has now been hidden
    }

    private checkStateAndToggleSetting(settingId: string, targetState: boolean) {
        console.log("toggleSetting for ", settingId);
        return this.getSetting(settingId).isSelected()
            .then((selected) => {
                if (selected !== targetState) {
                    this.findAndClickSetting(settingId);
                }
            });
    }

    private navigateToDetailsPage(name: string) {
        let navigationPromise;

        if (this.knownEntityPath === null) {
            navigationPromise = this.config.navigateToEntity(name)
                .then(() => browser.getCurrentUrl())
                .then(url => this.knownEntityPath = url.substr(browser.params.baseUrl.length));
        } else {
            // Save some time going directly to the contract in stead of going through kendo
            navigationPromise = this.navigation.getPage(this.knownEntityPath);
        }
        return navigationPromise;
    }

    private toggleSetting(settingId: string) {
        console.log("toggleSetting for ", settingId);
        return this.openUiConfiguration()
            .then(() => this.findAndClickSetting(settingId));
    }

    private openUiConfiguration() {
        return this.navigation.getPage(`/#/local-config/${this.config.localAdminArea}`)
            .then(() => element(by.id(`expand_collapse_${this.config.module}`)).click())
            .then(() => browser.waitForAngular());
    }

    private findAndClickSetting(settingId: string) {
        return this.getSetting(settingId).click()
            .then(() => browser.waitForAngular());
    }

    private getSetting(settingId: string) {
        return element(by.id(settingId));
    }

    private verifyTabVisibility(name: string, tabSref: string, expectedToBePresent: boolean) {
        console.log("verifyTabVisibility for ", name, " and tabSref:", tabSref, " expectedPresence:", expectedToBePresent);

        return this.navigateToDetailsPage(name)
            .then(() => expect(this.navigation.findSubMenuElement(tabSref).isPresent()).toBe(expectedToBePresent, `Failed to validate tab:${tabSref} to be ${expectedToBePresent ? "_present_" : "_removed_"}`));
    }

    private createName(prefix: string) {
        return `${prefix}_${new Date().getTime()}`;
    }

    private verifySettingVisibility(name: string, tabSref: string, settingElementIds: Array<string>, expectedToBePresent: boolean) {
        console.log("verifySettingVisibility for ", name, " and fields ", settingElementIds.join(", "), " located on tabSref:", tabSref, " expectedPresence:", expectedToBePresent);

        return this.navigateToDetailsPage(name)
            .then(() => expect(this.navigation.findSubMenuElement(tabSref).isPresent()).toBe(true, `Tab ${tabSref} is not present`))
            .then(() => this.navigation.findSubMenuElement(tabSref).click())
            .then(() => browser.waitForAngular())
            .then(() => {
                for (let settingElementId of settingElementIds) {
                    expect(element(by.id(settingElementId)).isPresent()).toBe(expectedToBePresent, `Setting: ${settingElementId} failed to meet expected visibility of ${expectedToBePresent}`);
                }
            });
    }
}