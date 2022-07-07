import Login = require("../../Helpers/LoginHelper");
import LocalDataProcessing = require("../../PageObjects/Local-admin/LocalDataProcessing.po");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import NavigationBarHelper = require("../../object-wrappers/navigationBarWrapper");
import SystemCatalogHelper = require("../../Helpers/SystemCatalogHelper");
import SystemUsageHelper = require("../../Helpers/SystemUsageHelper");
//import NavigationHelper = require("../../Utility/NavigationHelper");

describe("Local admin is able to toggle DataProcessing", () => {

    var loginHelper = new Login();
    var dpPageHelper = new LocalDataProcessing();
    var testFixture = new TestFixtureWrapper();
    var navigationBarHelper = new NavigationBarHelper();
    var systemName = createName("SystemName");
    //var navigationHelper = new NavigationHelper();

    beforeAll(() => {
        testFixture.enableLongRunningTest();
        return loginHelper.loginAsGlobalAdmin()
            .then(() => SystemCatalogHelper.createSystem(systemName))
            .then(() => SystemCatalogHelper.createLocalSystem(systemName))
            .then(() => loginHelper.logout())
            .then(() => loginHelper.loginAsLocalAdmin());
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
        console.log(`Checking DPR visibility, expected: ${visibility}, sysName: ${sysName}`);
        SystemUsageHelper.openLocalSystem(sysName)
            .then(() => expect(navigationBarHelper.headerNavigations.dataProcessingButton.isPresent()).toBe(visibility));
    }

    function expectCheckboxValueToBe(currentValueIs: boolean) {
        console.log(`Expecting Checkbox value to be: ${currentValueIs}`);
        expect((dpPageHelper.getToggleDataProcessingCheckbox()).isSelected()).toBe(currentValueIs);
    }

    function expectSystemGdprDataProcessingViewToBe(shown: boolean) {
        console.log(`Expecting Data processing view to be: ${shown}`);
        expect((navigationBarHelper.headerNavigations.dataProcessingButton).isPresent()).toBe(shown);
    }

    function createName(prefix: string) {
        return `${prefix}_${new Date().getTime()}`;
    }
});


