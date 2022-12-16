import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import SystemCatalogHelper = require("../../../Helpers/SystemCatalogHelper");
import SystemUsageHelper = require("../../../Helpers/SystemUsageHelper");
import NavigationHelper = require("../../../Utility/NavigationHelper");
import LocalItSystemNavigationSrefs = require("../../../Helpers/SideNavigation/LocalItSystemNavigationSrefs");
import Select2Helper = require("../../../Helpers/Select2Helper");
import Constants = require("../../../Utility/Constants");
import { UiCustomizationTestHelper } from "../../../Helpers/ui-customization-test-helper";

describe("Local admin is able customize the IT-System usage UI", () => {

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

    function createHelper() {
        return new UiCustomizationTestHelper({
            createEntity: (systemName) =>
                SystemCatalogHelper.createSystem(systemName)
                    .then(() => SystemCatalogHelper.createLocalSystem(systemName))
                    .then(() => SystemUsageHelper.openLocalSystem(systemName))
                    .then(() => navigation.findSubMenuElement(LocalItSystemNavigationSrefs.GDPRSref).click()) //Make sure hidden fields are available
                    .then(() => Select2Helper.selectWithNoSearch("Ja", consts.gdprRiskAssessmentSelect2Id)),
            navigateToEntity: systemName => SystemUsageHelper.openLocalSystem(systemName),
            localAdminArea: "system",
            module: "ItSystemUsages"
        });
    }

    it("Disabling Tabs/fields will hide the tabs/fields on the IT-System details page", () => {
        const helper = createHelper();
        return helper.setupUserAndOrg()
            .then(() => helper.testFieldCustomization("ItSystemUsages.gdpr.plannedRiskAssessmentDate", LocalItSystemNavigationSrefs.GDPRSref, "plannedRiskAssessmentDate"))
            .then(() => helper.testTabCustomization("ItSystemUsages.gdpr", LocalItSystemNavigationSrefs.GDPRSref))
            .then(() => helper.testTabCustomization("ItSystemUsages.advice", LocalItSystemNavigationSrefs.adviceSref))
            .then(() => helper.testTabCustomization("ItSystemUsages.archiving", LocalItSystemNavigationSrefs.archivingSref))
            .then(() => helper.testTabCustomization("ItSystemUsages.interfaces", LocalItSystemNavigationSrefs.exposedInterfacesSref))
            .then(() => helper.testTabCustomization("ItSystemUsages.systemRelations", LocalItSystemNavigationSrefs.relationsSref))
            .then(() => helper.testTabCustomization("ItSystemUsages.organization", LocalItSystemNavigationSrefs.organizationSref))
            .then(() => helper.testTabCustomization("ItSystemUsages.hierarchy", LocalItSystemNavigationSrefs.hierarchySref))
            .then(() => helper.testTabCustomization("ItSystemUsages.systemRoles", LocalItSystemNavigationSrefs.rolesSref))
            .then(() => helper.testTabCustomization("ItSystemUsages.localKle", LocalItSystemNavigationSrefs.kleSref))
            .then(() => helper.testTabCustomization("ItSystemUsages.localReferences", LocalItSystemNavigationSrefs.referencesSref))
            .then(() => helper.testFieldCustomization("ItSystemUsages.contracts.selectContractToDetermineIfItSystemIsActive", LocalItSystemNavigationSrefs.contractsSref, "selectMainContractSection"))
            .then(() => helper.testFieldCustomization("ItSystemUsages.frontPage.lifeCycleStatus", LocalItSystemNavigationSrefs.mainPageSref, "lifeCycleStatus"))
            .then(() => helper.testFieldGroupCustomization("ItSystemUsages.frontPage.usagePeriod", LocalItSystemNavigationSrefs.mainPageSref, ["agreement-concluded", "agreement-expiration"]));
    });
});


