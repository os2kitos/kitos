import Login = require("../../Helpers/LoginHelper");
import LocalDataProcessing = require("../../PageObjects/Local-admin/LocalDataProcessing.po");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import NavigationBarHelper = require("../../object-wrappers/navigationBarWrapper");
import SystemCatalogHelper = require("../../Helpers/SystemCatalogHelper");
import SystemUsageHelper = require("../../Helpers/SystemUsageHelper");
import LocalItSystemNavigation = require("../../Helpers/SideNavigation/LocalItSystemNavigation");
import SystemTabGDPR = require("../../PageObjects/it-system/Usage/Tabs/ItSystemUsageGDPR.po");

describe("Local admin is able to toggle DataProcessing", () => {

    var loginHelper = new Login();
    var dpPageHelper = new LocalDataProcessing();
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

    it("Option to toggle DataProcessing is visible", () => {
        dpPageHelper.getPage()
            .then(() => expect((dpPageHelper.getToggleDataProcessingCheckbox()).isPresent()).toBe(true));
    });

    it("Is able to toggle DataProcessing checkbox", () => {
        var isDataProcessingEnabled;
        dpPageHelper.getPage()
            .then(async () => isDataProcessingEnabled = await dpPageHelper.getToggleDataProcessingCheckbox().isSelected())
            .then(() => checkSystemGdprPageDataProcessingVisibility(isDataProcessingEnabled, systemName))
            .then(() => dpPageHelper.getPage())
            .then(() => expectCheckboxValueToBe(isDataProcessingEnabled))
            .then(() => expectSystemGdprDataProcessingViewToBe(isDataProcessingEnabled))
            .then(() => dpPageHelper.getToggleDataProcessingCheckbox().click())
            .then(() => browser.waitForAngular())
            .then(() => expectCheckboxValueToBe(!isDataProcessingEnabled))
            .then(() => expectSystemGdprDataProcessingViewToBe(!isDataProcessingEnabled) )
            .then(() => checkSystemGdprPageDataProcessingVisibility(!isDataProcessingEnabled, systemName));

    });

    function checkSystemGdprPageDataProcessingVisibility(visibility: boolean, sysName: string) {
        SystemUsageHelper.openLocalSystem(sysName)
            .then(() => LocalItSystemNavigation.openGDPRPage())
            .then(() => expect((SystemTabGDPR.getDataProcessingRegistrationView()).isPresent()).toBe(visibility));
    }

    function expectCheckboxValueToBe(currentValueIs: boolean) {
        expect((dpPageHelper.getToggleDataProcessingCheckbox()).isSelected()).toBe(currentValueIs);
    }

    function expectSystemGdprDataProcessingViewToBe(shown: boolean) {
        expect((naviHelper.headerNavigations.dataProcessingButton).isPresent()).toBe(shown);
    }

    function createItSystemName(index: number) {
        return `ItSystem${new Date().getTime()}_${index}`;
    }
});


