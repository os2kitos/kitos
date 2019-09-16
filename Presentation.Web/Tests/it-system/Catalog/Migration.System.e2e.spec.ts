import Login = require("../../Helpers/LoginHelper");
import CatalogHelper = require("../../Helpers/CatalogHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import Constants = require("../../Utility/Constants");
import CssHelper = require("../../Object-wrappers/CSSLocatorHelper");
import CatalogPage = require("../../PageObjects/it-system/Catalog/ItSystemCatalog.po");

describe("Global Administrator is able to migrate from one system to another", () => {
    var loginHelper = new Login();
    var testFixture = new TestFixtureWrapper();
    var cataHelper = CatalogHelper;
    var constants = new Constants();
    var cssHelper = new CssHelper();
    var pageObject = new CatalogPage();

    afterEach(() => {
        testFixture.cleanupState();
    });

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
    });

    it("Global Administrator is able to see the move button", () => {
        loginHelper.loginAsGlobalAdmin();
        checkMigrationButtonVisibility(true);
    });

    it("Local admin is not able to see the move button", () => {
        loginHelper.loginAsLocalAdmin();
        checkMigrationButtonVisibility(false);
    });

    it("Regular user is not able to see the move button", () => {
        loginHelper.loginAsRegularUser();
        checkMigrationButtonVisibility(false);
    });

    it("Global admin is able to get to the final migration window and execute a migration", () => {
        loginHelper.loginAsGlobalAdmin();

        var systemNameFrom = createItSystemName();
        var systemNameTo = createItSystemName() + 1;
        return pageObject.getPage()
            .then(() => cataHelper.waitForKendoGrid())
            .then(() => cataHelper.createCatalog(systemNameFrom))
            .then(() => cataHelper.createCatalog(systemNameTo))
            .then(() => toggleSystemActivation(systemNameFrom))
            .then(() => openMigrationOnSpecificSystem(systemNameFrom))
            .then(() => waitForModalWindowAnimation(constants.moveSystemButton))
            .then(() => expect(element(cssHelper.byDataElementType(constants.moveSystemButton)).isPresent()).toBe(true))
            .then(() => element(cssHelper.byDataElementType(constants.moveSystemButton)).click())
            .then(() => waitForModalWindowAnimation(constants.consequenceButton))
            .then(() => expect(element(cssHelper.byDataElementType(constants.consequenceButton)).isPresent()).toBe(true))
            .then(() => select2SearchForSystem(systemNameTo))
            .then(() => waitForSelect2DataAndSelect())
            .then(() => element(cssHelper.byDataElementType(constants.consequenceButton)).click())
            .then(() => waitForModalWindowAnimation(constants.startMigrationButton))
            .then(() => expect(element(cssHelper.byDataElementType(constants.startMigrationButton)).isDisplayed()).toBe(true))
            .then(() => element(cssHelper.byDataElementType(constants.startMigrationButton)).click())
            .then(() => expect(element(cssHelper.byDataElementType(constants.startMigrationButton)).isDisplayed()).toBe(false));
    });

    function createItSystemName() {
        return "It-System" + new Date().getTime();
    }

    function toggleSystemActivation(name: string) {
        return pageObject.getPage()
            .then(() => element(by.xpath('//*/tbody/*/td/a[text()="' + name + '"]/parent::*/parent::*//*/button')).click());
    }

    function openMigrationOnSpecificSystem(name: string) {
        return pageObject.getPage()
            .then(() => element(by.xpath('//*/tbody/*/td/a[text()="' + name + '"]/parent::*/parent::*//*/a[@data-element-type="usagesLinkText"]')).click());
    }

    function waitForModalWindowAnimation(name: string) {
        var EC = protractor.ExpectedConditions;
        return browser.wait(EC.visibilityOf(element(cssHelper.byDataElementType(name))),
            20000);
    }

    function waitForSelect2DataAndSelect() {
        var EC = protractor.ExpectedConditions;
        return browser.wait(EC.visibilityOf(element(by.className("select2-result-label"))), 20000)
            .then(() => element(by.className("select2-input")).sendKeys(protractor.Key.ENTER));
    }

    function select2SearchForSystem(name: string) {
        return element(by.className("select2-choice")).click()
            .then(() => element(by.className("select2-input")).click())
            .then(() => element(by.className("select2-input")).sendKeys(name));
    }

    function checkMigrationButtonVisibility(isVisible: boolean) {
        return pageObject.getPage()
            .then(() => cataHelper.waitForKendoGrid())
            .then(() => pageObject.kendoToolbarWrapper.columnObjects().usedByName.first().click())
            .then(() => expect(element(cssHelper.byDataElementType(constants.moveSystemButton)).isPresent()).toBe(isVisible));
    }
});