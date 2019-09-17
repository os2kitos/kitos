import Login = require("../../Helpers/LoginHelper");
import SystemCatalogHelper = require("../../Helpers/SystemCatalogHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import Constants = require("../../Utility/Constants");
import CssHelper = require("../../Object-wrappers/CSSLocatorHelper");
import CatalogPage = require("../../PageObjects/it-system/Catalog/ItSystemCatalog.po");

describe("Global Administrator is able to migrate from one system to another", () => {
    var loginHelper = new Login();
    var testFixture = new TestFixtureWrapper();
    var constants = new Constants();
    var cssHelper = new CssHelper();
    var pageObject = new CatalogPage();
    var EC = protractor.ExpectedConditions;

    afterEach(() => {
        testFixture.cleanupState();
    });

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
    });

    it("Local admin is not able to see the move button", () => {
        loginHelper.loginAsLocalAdmin()
            .then(() => pageObject.getPage())
            .then(() => openMigrationOnSpecificSystem("DefaultTestItSystem"))
            .then(() => expect(element(cssHelper.byDataElementType(constants.moveSystemButton)).isPresent()).toBe(false));
    });

    it("Regular user is not able to see the move button", () => {
        loginHelper.loginAsRegularUser()
            .then(() => pageObject.getPage())
            .then(() => openMigrationOnSpecificSystem("DefaultTestItSystem"))
            .then(() => expect(element(cssHelper.byDataElementType(constants.moveSystemButton)).isPresent()).toBe(false));
    });

    it("Global admin is able to get to the final migration window and execute a migration", () => {
        var systemNameFrom = createItSystemName(1);
        var systemNameTo = createItSystemName(2);
        loginHelper.loginAsGlobalAdmin()
            .then(() => pageObject.getPage())
            .then(() => SystemCatalogHelper.createSystem(systemNameFrom))
            .then(() => SystemCatalogHelper.createSystem(systemNameTo))
            .then(() => toggleSystemActivation(systemNameFrom))
            .then(() => openMigrationOnSpecificSystem(systemNameFrom))
            .then(() => waitForElement(constants.moveSystemButton))
            .then(() => expect(element(cssHelper.byDataElementType(constants.moveSystemButton)).isPresent()).toBe(true))
            .then(() => element(cssHelper.byDataElementType(constants.moveSystemButton)).click())
            .then(() => waitForElement(constants.consequenceButton))
            .then(() => expect(element(cssHelper.byDataElementType(constants.consequenceButton)).isPresent()).toBe(true))
            .then(() => select2SearchForSystem(systemNameTo))
            .then(() => waitForSelect2DataAndSelect())
            .then(() => element(cssHelper.byDataElementType(constants.consequenceButton)).click())
            .then(() => waitForElement(constants.startMigrationButton))
            .then(() => expect(element(cssHelper.byDataElementType(constants.startMigrationButton)).isDisplayed()).toBe(true))
            .then(() => element(cssHelper.byDataElementType(constants.startMigrationButton)).click())
            .then(() => expect(element(cssHelper.byDataElementType(constants.startMigrationButton)).isDisplayed()).toBe(false));
    });

    function createItSystemName(index: number) {
        return `ItSystem${new Date().getTime()}_${index}`;
    }

    function toggleSystemActivation(name: string) {
        console.log(`toggleSystemActivation: ${name}`);
        return pageObject.getPage()
            .then(() => element(by.xpath('//*/tbody/*/td/a[text()="' + name + '"]/parent::*/parent::*//*/button')).click());
    }

    function openMigrationOnSpecificSystem(name: string) {
        console.log(`openMigrationOnSpecificSystem: ${name}`);
        return pageObject.getPage()
            .then(() => element(by.xpath('//*/tbody/*/td/a[text()="' + name + '"]/parent::*/parent::*//*/a[@data-element-type="usagesLinkText"]')).click());
    }

    function waitForElement(name: string) {
        console.log(`waitForElement: ${name}`);
        return browser.wait(EC.visibilityOf(element(cssHelper.byDataElementType(name))),
            20000);
    }

    function waitForSelect2DataAndSelect() {
        console.log(`waitForSelect2DataAndSelect`);
        return browser.wait(EC.visibilityOf(element(by.className("select2-result-label"))), 20000)
            .then(() => element(by.className("select2-input")).sendKeys(protractor.Key.ENTER));
    }

    function select2SearchForSystem(name: string) {
        console.log(`select2SearchForSystem: ${name}`);
        return element(by.className("select2-choice")).click()
            .then(() => element(by.className("select2-input")).click())
            .then(() => element(by.className("select2-input")).sendKeys(name));
    }
});