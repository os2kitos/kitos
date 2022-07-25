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

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    var localContractPath: string | null = null;

    it("Disabling Tabs/fields will hide the tabs/fields on the IT-Contract details page", () => {
        var contractName = createName("contractName");
        var orgName = createName("orgName");

        return loginHelper.loginAsGlobalAdmin()
            .then(() => OrgHelper.createOrg(orgName))
            .then(() => OrgHelper.changeOrg(orgName))
            .then(() => ContractHelper.createContract(contractName))
            .then(() => navigation.getPage("/#/global-admin/local-admins"))
            .then(() => Select2Helper.select(orgName, "s2id_selectOrg"))
            .then(() => Select2Helper.select(loginHelper.getGlobalAdminCredentials().username, "selectUser"))
            .then(() => testTabCustomization(contractName, "ItContracts.contractRoles", ContractNavigationSrefs.contractRolesSref))
            .then(() => testTabCustomization(contractName, "ItContracts.advice", ContractNavigationSrefs.adviceSref))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.procurementPlan", ContractNavigationSrefs.frontPageSref, "selectProcurementPlan"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.procurementStrategy", ContractNavigationSrefs.frontPageSref, "selectProcurementStrategy"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.contractId", ContractNavigationSrefs.frontPageSref, "contract-id"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.contractType", ContractNavigationSrefs.frontPageSref, "s2id_contract-type"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.purchaseForm", ContractNavigationSrefs.frontPageSref, "s2id_contract-purchaseform"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.extSigner", ContractNavigationSrefs.frontPageSref, "contract-ext-signer"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.extSigned", ContractNavigationSrefs.frontPageSref, "contract-ext-signed"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.extDate", ContractNavigationSrefs.frontPageSref, "contract-ext-date"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.intSigner", ContractNavigationSrefs.frontPageSref, "contract-int-signer"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.intSigned", ContractNavigationSrefs.frontPageSref, "contract-int-signed"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.intDate", ContractNavigationSrefs.frontPageSref, "contract-int-date"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.agreementConcluded", ContractNavigationSrefs.frontPageSref, "agreement-concluded"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.agreementExpiration", ContractNavigationSrefs.frontPageSref, "agreement-expiration"))
            .then(() => testFieldCustomization(contractName, "ItContracts.frontPage.isActive", ContractNavigationSrefs.frontPageSref, "contractIsActive"))
            .then(() => testFieldCustomization(contractName, "ItContracts.deadlines.agreementDuration", ContractNavigationSrefs.deadlinesPageSref, "agreement-duration"))
            .then(() => testFieldCustomization(contractName, "ItContracts.deadlines.agreementDurationOnGoing", ContractNavigationSrefs.deadlinesPageSref, "idl"))
            .then(() => testFieldCustomization(contractName, "ItContracts.deadlines.optionExtend", ContractNavigationSrefs.deadlinesPageSref, "s2id_agreement-option-extend"))
            .then(() => testFieldCustomization(contractName, "ItContracts.deadlines.optionExtendMultiplier", ContractNavigationSrefs.deadlinesPageSref, "agreement-option-extend-multiplier"))
            .then(() => testFieldCustomization(contractName, "ItContracts.deadlines.irrevocable", ContractNavigationSrefs.deadlinesPageSref, "agreement-irrevocable"))
            .then(() => testFieldCustomization(contractName, "ItContracts.deadlines.agreementTerminated", ContractNavigationSrefs.deadlinesPageSref, "agreement-terminated"))
            .then(() => testFieldCustomization(contractName, "ItContracts.deadlines.agreementNotice", ContractNavigationSrefs.deadlinesPageSref, "s2id_agreement-notice"))
            .then(() => testFieldCustomization(contractName, "ItContracts.deadlines.agreementRunning", ContractNavigationSrefs.deadlinesPageSref, "s2id_agreement-running"))
            .then(() => testFieldCustomization(contractName, "ItContracts.deadlines.agreementByEnding", ContractNavigationSrefs.deadlinesPageSref, "s2id_agreement-by-ending"))
            .then(() => testFieldCustomization(contractName, "ItContracts.economy.paymentModel", ContractNavigationSrefs.economyPageSref, "payment-model"))
            .then(() => testFieldCustomization(contractName, "ItContracts.economy.extPayment", ContractNavigationSrefs.economyPageSref, "ext-payment"))
            .then(() => testFieldCustomization(contractName, "ItContracts.economy.intPayment", ContractNavigationSrefs.economyPageSref, "int-payment"))
            ;
    });

    function testTabCustomization(name: string, settingId: string, tabSref: string) {
        console.log("testTabCustomization for ", name, " and tabSref:", tabSref, " settingId:", settingId);
        return verifyTabVisibility(name, tabSref, true)               //Check that the tab is visible before the change
            .then(() => toggleSetting(settingId))                           //Toggle the setting
            .then(() => verifyTabVisibility(name, tabSref, false));   //Verify that the tab has now been hidden
    }

    function testFieldCustomization(contractName: string, settingId: string, tabSref: string, settingElementId: string) {
        console.log("testFieldCustomization for ", contractName, " and tabSref:", tabSref, " settingId:", settingId);
        return verifySettingVisibility(contractName, tabSref, settingElementId, true)                //Check that the setting is visible before the change
            .then(() => toggleSetting(settingId))                                                   //Toggle the setting
            .then(() => verifySettingVisibility(contractName, tabSref, settingElementId, false));     //Verify that the setting has now been hidden
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

    function verifySettingVisibility(contractName: string, tabSref: string, settingElementId: string, expectedToBePresent: boolean) {
        console.log("verifySettingVisibility for ", contractName, " and field", settingElementId, " located on tabSref:", tabSref, " expectedPresence:", expectedToBePresent);

        return navigateToContract(contractName)
            .then(() => expect(navigation.findSubMenuElement(tabSref).isPresent()).toBe(true, `Tab ${tabSref} containing setting ${settingElementId} is not present`))
            .then(() => navigation.findSubMenuElement(tabSref).click())
            .then(() => browser.waitForAngular())
            .then(() => expect(element(by.id(settingElementId)).isPresent()).toBe(expectedToBePresent, `Setting: ${settingElementId} failed to meet expected visibility of ${expectedToBePresent}`));
    }

    function toggleSetting(settingId: string) {
        console.log("toggleSetting for ", settingId);
        return navigation.getPage("/#/local-config/contract")
            .then(() => element(by.id("expand_collapse_ItContracts")).click())
            .then(() => browser.waitForAngular())
            .then(() => element(by.id(settingId)).click())
            .then(() => browser.waitForAngular());
    }

    function createName(prefix: string) {
        return `${prefix}_${new Date().getTime()}`;
    }
});


