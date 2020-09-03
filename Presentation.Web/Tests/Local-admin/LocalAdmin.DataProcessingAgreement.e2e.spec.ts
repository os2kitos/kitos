import Login = require("../Helpers/LoginHelper");
import DPA = require("../PageObjects/Local-admin/LocalDataProcessingAgreement.po");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import NavigationBarHelper = require("../object-wrappers/navigationBarWrapper");
import SystemCatalogHelper = require("../Helpers/SystemCatalogHelper");
import SystemUsageHelper = require("../Helpers/SystemUsageHelper");
import LocalItSystemNavigation = require("../Helpers/SideNavigation/LocalItSystemNavigation");
import SystemTabGDPR = require("../PageObjects/it-system/Usage/Tabs/ItSystemUsageGDPR.po");

describe("Local admin is able to toggle DataProcessingAgreement", () => {

    var loginHelper = new Login();
    var dpaPageHelper = new DPA();
    var testFixture = new TestFixtureWrapper();
    var naviHelper = new NavigationBarHelper();
    var systemName = createItSystemName(1);

    beforeAll(() => {
        testFixture.enableLongRunningTest();
        loginHelper.loginAsGlobalAdmin();
        SystemCatalogHelper.createSystem(systemName);
        SystemCatalogHelper.createLocalSystem(systemName);
        loginHelper.logout();
        loginHelper.loginAsLocalAdmin();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("Option to toggle DataProcessingAgreement is visible", () => {
        dpaPageHelper.getPage()
            .then(() => browser.waitForAngular())
            .then(() => expect((dpaPageHelper.getToggleDataProcessingAgreementCheckbox()).isPresent()).toBe(true));
    });

    it("Is able to toggle DataProcessingAgreement checkbox", () => {
        var boolCheckBox;
        dpaPageHelper.getPage()
            .then(() => browser.waitForAngular())
            .then(async () => boolCheckBox = await dpaPageHelper.getToggleDataProcessingAgreementCheckbox().isSelected())
            .then(() => checkSystemGdprPageDataProcessingAgreementVisibility(boolCheckBox, systemName))
            .then(() => dpaPageHelper.getPage())
            .then(() => browser.waitForAngular())
            .then(() => expectCheckboxValueTobe(boolCheckBox))
            .then(() => expectSystemGdprDataProcessingAgreementViewToBe(boolCheckBox))
            .then(() => dpaPageHelper.getToggleDataProcessingAgreementCheckbox().click())
            .then(() => browser.waitForAngular())
            .then(() => browser.refresh())
            .then(() => expectCheckboxValueTobe(!boolCheckBox))
            .then(() => expectSystemGdprDataProcessingAgreementViewToBe(!boolCheckBox) )
            .then(() => checkSystemGdprPageDataProcessingAgreementVisibility(!boolCheckBox, systemName));

    });

    function checkSystemGdprPageDataProcessingAgreementVisibility(visibility: boolean, sysName: string) {
        SystemUsageHelper.openLocalSystem(sysName)
            .then(() => LocalItSystemNavigation.openGDPRPage())
            .then(() => browser.waitForAngular())
            .then(() => expect((SystemTabGDPR.getDataProcessingAgreementView()).isPresent()).toBe(visibility));
    }

    function expectCheckboxValueTobe(currentValueIs: boolean) {
        expect((dpaPageHelper.getToggleDataProcessingAgreementCheckbox()).isSelected()).toBe(currentValueIs);
    }

    function expectSystemGdprDataProcessingAgreementViewToBe(shown: boolean) {
        expect((naviHelper.headerNavigations.dataProcessingAgreementButton).isPresent()).toBe(shown);
    }

    function createItSystemName(index: number) {
        return `ItSystem${new Date().getTime()}_${index}`;
    }
});


