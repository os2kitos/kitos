﻿import Login = require("../../../Helpers/LoginHelper");
import CatalogHelper = require("../../../Helpers/SystemCatalogHelper");
import ItSystemEditPo = require("../../../PageObjects/it-system/Catalog/ItSystemCatalog.po");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import CssHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import InterfaceHelper = require("../../../Helpers/InterfaceCatalogHelper");
import waitUpTo = require("../../../Utility/WaitTimers");
import ItSystemFrontPage = require("../../../PageObjects/It-system/Tabs/ItSystemFrontpage.po");

describe("Getting the correct message when there is a conflict deleting a system",
    () => {
        var loginHelper = new Login();
        var itSystemPage = new ItSystemEditPo();
        var itSystemFrontPage = new ItSystemFrontPage();
        var testFixture = new TestFixtureWrapper();
        var cssHelper = new CssHelper();
        var findCatalogColumnsFor = CatalogHelper.findCatalogColumnsFor;
        var EC = protractor.ExpectedConditions;
        var waitTimer = new waitUpTo();

        afterEach(() => {
            testFixture.enableAutoBrowserWaits();
        });

        beforeAll(() => {
            testFixture.enableLongRunningTest();
            loginHelper.loginAsGlobalAdmin();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        it("When system is deleted successfully",
            () => {
                var systemName = createName("SystemDeleteSuccess");
                loadPage()
                    .then(() => expectCreateButtonVisibility(true))
                    .then(() => CatalogHelper.createSystem(systemName))
                    .then(() => console.log(`Expecting system with name ${systemName}`))
                    .then(() => expectSystemWithName(systemName))
                    .then(() => CatalogHelper.getDeleteButtonForSystem(systemName))
                    .then(() => itSystemFrontPage.getDeleteButton().click())
                    .then(() => browser.wait(EC.alertIsPresent(), waitTimer.tenSeconds))
                    .then(() => browser.switchTo().alert().then(alert => {
                        testFixture.disableAutoBrowserWaits();
                        alert.accept();
                    }))
                    .then(() => console.log("Waiting for toast message"))
                    .then(() => waitForToastMessageToAppear("IT System er slettet!"));
            });

        it("When system is in use",
            () => {
                var systemName = createName("SystemInUse");

                loadPage()
                    .then(() => expectCreateButtonVisibility(true))
                    .then(() => CatalogHelper.createSystem(systemName))
                    .then(() => expectSystemWithName(systemName))
                    .then(() => toggleSystemInUse(systemName))
                    .then(() => CatalogHelper.getDeleteButtonForSystem(systemName))
                    .then(() => itSystemFrontPage.getDeleteButton().click())
                    .then(() => browser.wait(EC.alertIsPresent(), waitTimer.tenSeconds))
                    .then(() => browser.switchTo().alert().then(alert => {
                        testFixture.disableAutoBrowserWaits();
                        alert.accept();
                    }))
                    .then(() => waitForToastMessageToAppear("Systemet kan ikke slettes! Da Systemet er i anvendelse i en eller flere organisationer"));
            });

        it("When a interface depends on the system",
            () => {
                var systemName = createName("SystemWithInterface");
                var interfaceName = createName("Interface");

                loadPage()
                    .then(() => expectCreateButtonVisibility(true))
                    .then(() => CatalogHelper.createSystem(systemName))
                    .then(() => expectSystemWithName(systemName))
                    .then(() => InterfaceHelper.createInterface(interfaceName))
                    .then(() => InterfaceHelper.bindInterfaceToSystem(systemName, interfaceName))
                    .then(() => CatalogHelper.getDeleteButtonForSystem(systemName))
                    .then(() => itSystemFrontPage.getDeleteButton().click())
                    .then(() => browser.wait(EC.alertIsPresent(), waitTimer.tenSeconds))
                    .then(() => browser.switchTo().alert().then(alert => {
                        testFixture.disableAutoBrowserWaits();
                        alert.accept();
                    }))
                    .then(() => waitForToastMessageToAppear("Systemet kan ikke slettes! Da en det er markeret som udstillersystem for en eller flere snitflader"));
            });

        it("When another system depends on it",
            () => {
                var mainSystemName = createName("MainSystem");
                var childSystemName = createName("ChildSystem");

                loadPage()
                    .then(() => expectCreateButtonVisibility(true))
                    .then(() => CatalogHelper.createSystem(mainSystemName))
                    .then(() => console.log(`Expecting system with name ${mainSystemName}`))
                    .then(() => expectSystemWithName(mainSystemName))
                    .then(() => expectCreateButtonVisibility(true))
                    .then(() => CatalogHelper.createSystem(childSystemName))
                    .then(() => console.log(`Expecting system with name ${childSystemName}`))
                    .then(() => expectSystemWithName(childSystemName))
                    .then(() => CatalogHelper.setMainSystem(mainSystemName, childSystemName))
                    .then(() => CatalogHelper.getDeleteButtonForSystem(mainSystemName))
                    .then(() => itSystemFrontPage.getDeleteButton().click())
                    .then(() => browser.wait(EC.alertIsPresent(), waitTimer.tenSeconds))
                    .then(() => browser.switchTo().alert().then(alert => {
                        testFixture.disableAutoBrowserWaits();
                        alert.accept();
                    }))
                    .then(() => console.log("Waiting for toast message"))
                    .then(() => waitForToastMessageToAppear("Systemet kan ikke slettes! Da det er markeret som overordnet system for en eller flere systemer"));
            });


        function expectCreateButtonVisibility(expectedEnabledState: boolean) {
            console.log(`Expecting createCatalog visibility to be:${expectedEnabledState}`);
            return expect(itSystemPage.kendoToolbarWrapper.headerButtons().systemCatalogCreate.isEnabled()).toBe(expectedEnabledState);
        }

        function waitForKendoGrid() {
            return CatalogHelper.waitForKendoGrid();
        }

        function loadPage() {
            console.log("Loading system catalog page");
            return itSystemPage.getPage()
                .then(() => waitForKendoGrid());
        }

        function createName(prefix: string) {
            return `${prefix}_${new Date().getTime()}`;
        }

        function expectSystemWithName(name: string) {
            console.log(`Making sure ${name} does exist`);
            return CatalogHelper.waitForKendoGrid()
                .then(() => expect(findCatalogColumnsFor(name).first().getText()).toEqual(name));
        }

        function toggleSystemInUse(name: string) {
            return element(by.xpath(`//*/tbody/*/td/a[text()="${name}"]/parent::*/parent::*//button`)).click();
        }

        function getToastElement() {
            return element(cssHelper.byDataElementType("notification-message-block"));
        }

        function waitForToastMessageToAppear(msg: string) {
            return browser.waitForAngular()
                .then(() => browser.wait(getToastElement().isPresent(), waitTimer.twentySeconds))
                .then(() => browser.wait(EC.textToBePresentInElement(getToastElement(), msg), waitTimer.twentySeconds));
        }
    });