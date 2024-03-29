﻿import Login = require("../../../Helpers/LoginHelper");
import SystemCatalogHelper = require("../../../Helpers/SystemCatalogHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import Constants = require("../../../Utility/Constants");
import CssHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import CatalogPage = require("../../../PageObjects/it-system/Catalog/ItSystemCatalog.po");
import WaitTimers = require("../../../Utility/WaitTimers");

describe("Global Administrator is able to migrate from one system to another", () => {
    var loginHelper = new Login();
    var testFixture = new TestFixtureWrapper();
    var constants = new Constants();
    var cssHelper = new CssHelper();
    var pageObject = new CatalogPage();
    var ec = protractor.ExpectedConditions;
    var waitUpTo = new WaitTimers();

    afterEach(() => {
        testFixture.cleanupState();
    });

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
    });

    it("Global admin is able to get to the final migration window and execute a migration", () => {
        var systemNameFrom = createName("FromSystem");
        var systemNameTo = createName("ToSystem");
        loginHelper.loginAsGlobalAdmin()
            .then(() => pageObject.getPage())
            .then(() => SystemCatalogHelper.createSystem(systemNameFrom))
            .then(() => SystemCatalogHelper.createSystem(systemNameTo))
            .then(() => SystemCatalogHelper.waitForKendoGrid())
            .then(() => toggleSystemActivation(systemNameFrom))
            .then(() => openMigrationOnSpecificSystem(systemNameFrom))
            .then(() => waitForButtonToBeClickAble(constants.moveSystemButton))
            .then(() => expect(element(cssHelper.byDataElementType(constants.moveSystemButton)).isPresent()).toBe(true))
            .then(() => element(cssHelper.byDataElementType(constants.moveSystemButton)).click())
            .then(() => waitForButtonToBeClickAble(constants.consequenceButton))
            .then(() => expect(element(cssHelper.byDataElementType(constants.consequenceButton)).isPresent()).toBe(true))
            .then(() => select2SearchForSystem(systemNameTo))
            .then(() => waitForSelect2DataAndSelect())
            .then(() => element(cssHelper.byDataElementType(constants.consequenceButton)).click())
            .then(() => waitForButtonToBeClickAble(constants.startMigrationButton))
            .then(() => expect(element(cssHelper.byDataElementType(constants.startMigrationButton)).isDisplayed()).toBe(true))
            .then(() => element(cssHelper.byDataElementType(constants.startMigrationButton)).click())
            .then(() => expect(element(cssHelper.byDataElementType(constants.startMigrationButton)).isDisplayed()).toBe(false));
    });

    function createName(prefix: string) {
        return `${prefix}_${new Date().getTime()}`;
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

    function waitForButtonToBeClickAble(name: string) {
        console.log(`Waiting for button to be clickable: ${name}`);
        return browser.wait(ec.elementToBeClickable(element(cssHelper.byDataElementType(name))), waitUpTo.twentySeconds);
    }

    function waitForSelect2DataAndSelect() {
        console.log(`waitForSelect2DataAndSelect`);
        return browser.wait(ec.visibilityOf(element(by.className("select2-result-label"))), waitUpTo.twentySeconds)
            .then(() => element(by.className("select2-input")).sendKeys(protractor.Key.ENTER));
    }

    function select2SearchForSystem(name: string) {
        console.log(`select2SearchForSystem: ${name}`);
        return element(by.className("select2-choice")).click()
            .then(() => element(by.className("select2-input")).click())
            .then(() => element(by.className("select2-input")).sendKeys(name));
    }

});