import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import ContractHelper = require("../../Helpers/ContractHelper");
import ContractNavigationSrefs = require("../../Helpers/SideNavigation/ContractNavigationSrefs");
import { UiCustomizationTestHelper } from "../../Helpers/ui-customization-test-helper";

describe("Local admin is able customize the IT-Contract UI", () => {

    var testFixture = new TestFixtureWrapper();

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterEach(() => {
        testFixture.cleanupState();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    function createHelper() {
        return new UiCustomizationTestHelper({
            createEntity: contractName => ContractHelper.createContract(contractName),
            navigateToEntity: contractName => ContractHelper.openContract(contractName),
            localAdminArea: "contract",
            module: "ItContracts"
        });
    }


    it("Disabling tabs will hide the tabs on the IT-Contract details page", () => {

        const helper = createHelper();
        return helper.setupUserAndOrg()
            .then(() => helper.testTabCustomization("ItContracts.contractRoles", ContractNavigationSrefs.contractRolesSref))
            .then(() => helper.testTabCustomization("ItContracts.advice", ContractNavigationSrefs.adviceSref))
            .then(() => helper.testTabCustomization("ItContracts.economy", ContractNavigationSrefs.economyPageSref))
            .then(() => helper.testTabCustomization("ItContracts.deadlines", ContractNavigationSrefs.deadlinesPageSref));
    });

    it("Disabling fields will hide the fields on the IT-Contract page contents", () => {

        const helper = createHelper();
        return helper.setupUserAndOrg()
            // Front page
            .then(() => helper.testFieldCustomization("ItContracts.frontPage.procurementPlan", ContractNavigationSrefs.frontPageSref, "selectProcurementPlan"))
            .then(() => helper.testFieldCustomization("ItContracts.frontPage.procurementStrategy", ContractNavigationSrefs.frontPageSref, "selectProcurementStrategy"))
            .then(() => helper.testFieldCustomization("ItContracts.frontPage.procurementInitiated", ContractNavigationSrefs.frontPageSref, "selectProcurementInitiated"))
            .then(() => helper.testFieldCustomization("ItContracts.frontPage.contractId", ContractNavigationSrefs.frontPageSref, "contract-id"))
            .then(() => helper.testFieldCustomization("ItContracts.frontPage.contractType", ContractNavigationSrefs.frontPageSref, "s2id_contract-type"))
            .then(() => helper.testFieldCustomization("ItContracts.frontPage.purchaseForm", ContractNavigationSrefs.frontPageSref, "s2id_contract-purchaseform"))
            .then(() => helper.testFieldGroupCustomization("ItContracts.frontPage.externalSigner", ContractNavigationSrefs.frontPageSref, ["contract-ext-signer", "contract-ext-signed", "contract-ext-date"]))
            .then(() => helper.testFieldGroupCustomization("ItContracts.frontPage.internalSigner", ContractNavigationSrefs.frontPageSref, ["contract-int-signer", "contract-int-signed", "contract-int-date"]))
            .then(() => helper.testFieldGroupCustomization("ItContracts.frontPage.agreementPeriod", ContractNavigationSrefs.frontPageSref, ["agreement-concluded", "agreement-expiration"]))
            .then(() => helper.testFieldCustomization("ItContracts.frontPage.isActive", ContractNavigationSrefs.frontPageSref, "contractIsActive"))

            // Deadlines tab
            .then(() => helper.testFieldCustomizationWithSubtreeIsComplete("ItContracts.deadlines.agreementDeadlines", ContractNavigationSrefs.deadlinesPageSref, "agreement-deadlines"))
            .then(() => helper.testFieldCustomizationWithSubtreeIsComplete("ItContracts.deadlines.termination", ContractNavigationSrefs.deadlinesPageSref, "termination"))
            .then(() => helper.testTabCustomizationWithSubtreeIsComplete(["ItContracts.deadlines.agreementDeadlines", "ItContracts.deadlines.termination"], ContractNavigationSrefs.deadlinesPageSref))


            // Economy tab
            .then(() => helper.testFieldCustomizationWithSubtreeIsComplete("ItContracts.economy.paymentModel", ContractNavigationSrefs.economyPageSref, "payment-model"))
            .then(() => helper.testFieldCustomizationWithSubtreeIsComplete("ItContracts.economy.extPayment", ContractNavigationSrefs.economyPageSref, "ext-payment"))
            .then(() => helper.testFieldCustomizationWithSubtreeIsComplete("ItContracts.economy.intPayment", ContractNavigationSrefs.economyPageSref, "int-payment"))
            .then(() => helper.testTabCustomizationWithSubtreeIsComplete(["ItContracts.economy.paymentModel", "ItContracts.economy.extPayment", "ItContracts.economy.intPayment"], ContractNavigationSrefs.economyPageSref));
    });
});


