import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import ContractHelper = require("../../Helpers/ContractHelper");
import NavigationHelper = require("../../Utility/NavigationHelper");
import ContractNavigationSrefs = require("../../Helpers/SideNavigation/ContractNavigationSrefs");
import OrgHelper = require("../../Helpers/OrgHelper");
import Select2Helper = require("../../Helpers/Select2Helper");

describe("Local admin is able customize the IT-Contract UI", () => {
    
    var loginHelper = new Login();
    var testFixture = new TestFixtureWrapper();
    var navigation = new NavigationHelper();

    var contractName = "";
    var orgName = "";

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    beforeEach(() => {
        contractName = createName("contract");
        orgName = createName("org");
        console.log("Created contract name ", contractName, " and org name ", orgName);
    });

    afterEach(() => {
        testFixture.cleanupState();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    var localContractPath: string | null = null;

    it("Disabling tabs will hide the tabs on the IT-Contract details page", () => {

        return setupUserAndOrg()
            .then(() => testTabCustomization(contractName, "ItContracts.contractRoles", ContractNavigationSrefs.contractRolesSref))
            .then(() => testTabCustomization(contractName, "ItContracts.advice", ContractNavigationSrefs.adviceSref))
            .then(() => testTabCustomization(contractName, "ItContracts.economy", ContractNavigationSrefs.economyPageSref))
            .then(() => testTabCustomization(contractName, "ItContracts.deadlines", ContractNavigationSrefs.deadlinesPageSref));
    });

    it("Disabling fields will hide the fields on the IT-Contract page contents", () => {

        return setupUserAndOrg()
            // Front page
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.procurementPlan", ContractNavigationSrefs.frontPageSref, "selectProcurementPlan"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.procurementStrategy", ContractNavigationSrefs.frontPageSref, "selectProcurementStrategy"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.procurementInitiated", ContractNavigationSrefs.frontPageSref, "selectProcurementInitiated"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.contractId", ContractNavigationSrefs.frontPageSref, "contract-id"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.contractType", ContractNavigationSrefs.frontPageSref, "s2id_contract-type"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.purchaseForm", ContractNavigationSrefs.frontPageSref, "s2id_contract-purchaseform"))
            .then(() => testFieldGroupCustomization(contractName, "ItContracts.frontPage.externalSigner", ContractNavigationSrefs.frontPageSref, ["contract-ext-signer", "contract-ext-signed", "contract-ext-date"]))
            .then(() => testFieldGroupCustomization(contractName, "ItContracts.frontPage.internalSigner", ContractNavigationSrefs.frontPageSref, ["contract-int-signer", "contract-int-signed", "contract-int-date"]))
            .then(() => testFieldGroupCustomization(contractName, "ItContracts.frontPage.agreementPeriod", ContractNavigationSrefs.frontPageSref, ["agreement-concluded", "agreement-expiration"]))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.isActive", ContractNavigationSrefs.frontPageSref, "contractIsActive"))

            // Deadlines tab
            .then(() => testFieldCustomizationWithSubtreeIsComplete(contractName, "ItContracts.deadlines.agreementDeadlines", ContractNavigationSrefs.deadlinesPageSref, "agreement-deadlines"))
            .then(() => testFieldCustomizationWithSubtreeIsComplete(contractName, "ItContracts.deadlines.termination", ContractNavigationSrefs.deadlinesPageSref, "termination"))
            .then(() => testTabCustomizationWithSubtreeIsComplete(contractName, ["ItContracts.deadlines.agreementDeadlines", "ItContracts.deadlines.termination"], ContractNavigationSrefs.deadlinesPageSref))


            // Economy tab
            .then(() => testFieldCustomizationWithSubtreeIsComplete(contractName, "ItContracts.economy.paymentModel", ContractNavigationSrefs.economyPageSref, "payment-model"))
            .then(() => testFieldCustomizationWithSubtreeIsComplete(contractName, "ItContracts.economy.extPayment", ContractNavigationSrefs.economyPageSref, "ext-payment"))
            .then(() => testFieldCustomizationWithSubtreeIsComplete(contractName, "ItContracts.economy.intPayment", ContractNavigationSrefs.economyPageSref, "int-payment"))
            .then(() => testTabCustomizationWithSubtreeIsComplete(contractName, ["ItContracts.economy.paymentModel", "ItContracts.economy.extPayment", "ItContracts.economy.intPayment"], ContractNavigationSrefs.economyPageSref));
    });

    function setupUserAndOrg() {
        return loginHelper.loginAsGlobalAdmin()
            .then(() => OrgHelper.createOrg(orgName))
            .then(() => OrgHelper.changeOrg(orgName))
            .then(() => ContractHelper.createContract(contractName))
            .then(() => navigation.getPage("/#/global-admin/local-admins"))
            .then(() => Select2Helper.select(orgName, "s2id_selectOrg"))
            .then(() => Select2Helper.select(loginHelper.getGlobalAdminCredentials().username, "selectUser"));
    }

    function testTabCustomization(name: string, settingId: string, tabSref: string) {
        console.log("testTabCustomization for ", name, " and tabSref:", tabSref, " settingId:", settingId);
        return verifyTabVisibility(name, tabSref, true)               //Check that the tab is visible before the change
            .then(() => toggleSetting(settingId))                           //Toggle the setting
            .then(() => verifyTabVisibility(name, tabSref, false));   //Verify that the tab has now been hidden
    }

    function testTabCustomizationWithSubtreeIsComplete(name: string, settingIds: Array<string>, tabSref: string) {
        console.log("testTabCustomization for ", name, " and tabSref:", tabSref);
        
        return verifyTabVisibility(name, tabSref, true)                                                                   //Check that the tab is visible before the change
            .then(() => openUiConfiguration())                                                                            //Open Ui Customization page and show details
            .then(() => settingIds.reduce((promise, item) => {
                return promise.then(() => checkStateAndToggleSetting(item, false));
            }, protractor.promise.when([])))                                                                              //Toggle the setting
            .then(() => verifyTabVisibility(name, tabSref, false));                                                       //Verify that the tab has now been hidden
    }

    function testFieldCustomization(contractName: string, settingId: string, tabSref: string, settingElementId: string) {
        return testFieldGroupCustomization(contractName, settingId, tabSref, [settingElementId]);
    }

    function testFieldCustomizationWithSubtreeIsComplete(contractName: string, settingId: string, tabSref: string, settingElementId: string) {
        return testFieldGroupCustomizationWithSubtreeIsComplete(contractName, settingId, tabSref, [settingElementId]);
    }

    function testFieldGroupCustomization(contractName: string, settingId: string, tabSref: string, settingElementIds: Array<string>) {
        console.log("testFieldCustomization for ", contractName, " and tabSref:", tabSref, " affecting settings with ids:", settingElementIds.join(", "));
        return verifySettingVisibility(contractName, tabSref, settingElementIds, true)                //Check that the setting is visible before the change
            .then(() => toggleSetting(settingId))                                                   //Toggle the setting
            .then(() => verifySettingVisibility(contractName, tabSref, settingElementIds, false));     //Verify that the setting has now been hidden
    }

    function testFieldGroupCustomizationWithSubtreeIsComplete(contractName: string, settingId: string, tabSref: string, settingElementIds: Array<string>) {
        console.log("testFieldCustomization for ", contractName, " and tabSref:", tabSref, " affecting settings with ids:", settingElementIds.join(", "));
        return verifySettingVisibility(contractName, tabSref, settingElementIds, true)                //Check that the setting is visible before the change
            .then(() => toggleSetting(settingId))                                                     //Toggle the setting
            .then(() => verifySettingVisibility(contractName, tabSref, settingElementIds, false))     //Verify that the setting has now been hidden
            .then(() => toggleSetting(settingId));                                                    //Toggle the setting
    }

    function navigateToContract(contractName: string) {
        let navigationPromise;

        if (localContractPath === null) {
            navigationPromise = ContractHelper
                .openContract(contractName)
                .then(() => browser.getCurrentUrl())
                .then(url => localContractPath = url.substr(browser.params.baseUrl.length));
        } else {
            // Save some time going directly to the contract in stead of going through kendo
            navigationPromise = navigation.getPage(localContractPath);
        }
        return navigationPromise;
    }

    function verifyTabVisibility(contractName: string, tabSref: string, expectedToBePresent: boolean) {
        console.log("verifyTabVisibility for ", contractName, " and tabSref:", tabSref, " expectedPresence:", expectedToBePresent);

        return navigateToContract(contractName)
            .then(() => expect(navigation.findSubMenuElement(tabSref).isPresent()).toBe(expectedToBePresent, `Failed to validate tab:${tabSref} to be ${expectedToBePresent ? "_present_" : "_removed_"}`));
    }

    function verifySettingVisibility(contractName: string, tabSref: string, settingElementIds: Array<string>, expectedToBePresent: boolean) {
        console.log("verifySettingVisibility for ", contractName, " and fields ", settingElementIds.join(", "), " located on tabSref:", tabSref, " expectedPresence:", expectedToBePresent);

        return navigateToContract(contractName)
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
        return openUiConfiguration()
            .then(() => findAndClickSetting(settingId));
    }

    function checkStateAndToggleSetting(settingId: string, targetState: boolean) {
        console.log("toggleSetting for ", settingId);
        return getSetting(settingId).isSelected()
            .then((selected) => {
                if (selected !== targetState) {
                    findAndClickSetting(settingId);
                }
            });
    }

    function openUiConfiguration() {
        return navigation.getPage("/#/local-config/contract")
            .then(() => element(by.id("expand_collapse_ItContracts")).click())
            .then(() => browser.waitForAngular());
    }

    function findAndClickSetting(settingId: string) {
        return getSetting(settingId).click()
            .then(() => browser.waitForAngular());
    }

    function getSetting(settingId: string) {
        return element(by.id(settingId));
    }


    function createName(prefix: string) {
        return `${prefix}_${new Date().getTime()}`;
    }
});


