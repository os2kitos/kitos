import Login = require("../../Helpers/LoginHelper");
import CatalogHelper = require("../../Helpers/SystemCatalogHelper");
import ItSystemEditPo = require("../../PageObjects/it-system/Catalog/ItSystemCatalog.po")
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import CssHelper = require("../../Object-wrappers/CSSLocatorHelper");

describe("ITSystem Catalog accessibility tests", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemEditPo();
    var testFixture = new TestFixtureWrapper();
    var cssHelper = new CssHelper();
    var findCatalogColumnsFor = CatalogHelper.findCatalogColumnsFor;

    afterEach(() => {
        testFixture.cleanupState();
    });

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
    });

    it("Correct error message when succesfulle deleting system", () => {
        var systemName = createSystemName();
        var testFixture = new TestFixtureWrapper();

        loginHelper.loginAsGlobalAdmin()
            .then(() => loadPage())
            .then(() => waitForKendoGrid())
            .then(() => expectCreateButtonVisibility(true))
            .then(() => CatalogHelper.createSystem(systemName))
            .then(() => console.log("Expecting system with name " + systemName))
            .then(() => expectSystemWithName(systemName))
            .then(() => CatalogHelper.deleteSystemWithoutBrowserWait(systemName))
            .then(() => console.log("Waiting for toast message"))
            .then(() => browser.wait(getToastElement().isPresent(), 20000))
            .then(() => expect(getToastText()).toEqual("IT System  er slettet!"))
            .then(() => testFixture.enableAutoBrowserWaits());
    });

    it("Correct error message when trying to delete system in use", () => {
        var systemName = createSystemName();
        var testFixture = new TestFixtureWrapper();

        loginHelper.loginAsGlobalAdmin()
            .then(() => loadPage())
            .then(() => waitForKendoGrid())
            .then(() => expectCreateButtonVisibility(true))
            .then(() => CatalogHelper.createSystem(systemName))
            .then(() => console.log("Expecting system with name " + systemName))
            .then(() => expectSystemWithName(systemName))
            .then(() => console.log("Toggling system " + systemName))
            .then(() => toggleSystemInUse(systemName))
            .then(() => console.log("Trying to delete IT-System"))
            .then(() => CatalogHelper.deleteSystemWithoutBrowserWait(systemName))
            .then(() => console.log("Waiting for toast message"))
            .then(() => browser.wait(getToastElement().isPresent(), 20000))
            .then(() => expect(getToastText()).toEqual("Systemet kan ikke slettes! Da Systemet er i brug"))
            .then(() => testFixture.enableAutoBrowserWaits());

    });

    it("Correct error message when trying to delete system with child system", () => {
        var mainSystemName = "main" + createSystemName();
        var childSystemName = "child" + createSystemName();
        var testFixture = new TestFixtureWrapper();

        loginHelper.loginAsGlobalAdmin()
            .then(() => loadPage())
            .then(() => waitForKendoGrid())
            .then(() => expectCreateButtonVisibility(true))
            .then(() => CatalogHelper.createSystem(mainSystemName))
            .then(() => console.log("Expecting system with name " + mainSystemName))
            .then(() => expectSystemWithName(mainSystemName))
            .then(() => expectCreateButtonVisibility(true))
            .then(() => CatalogHelper.createSystem(childSystemName))
            .then(() => console.log("Expecting system with name " + childSystemName))
            .then(() => expectSystemWithName(childSystemName))
            .then(() => CatalogHelper.setMainSystem(mainSystemName, childSystemName))
            .then(() => CatalogHelper.deleteSystemWithoutBrowserWait(mainSystemName))
            .then(() => console.log("Waiting for toast message"))
            .then(() => browser.wait(getToastElement().isPresent(), 20000))
            .then(() => expect(getToastText()).toEqual("Systemet kan ikke slettes! Da andre systemer afhænger af dette system"))
            .then(() => testFixture.enableAutoBrowserWaits());
    });


    function expectCreateButtonVisibility(expectedEnabledState: boolean) {
        console.log("Expecting createCatalog visibility to be:" + expectedEnabledState);
        return expect(pageObject.kendoToolbarWrapper.headerButtons().systemCatalogCreate.isEnabled()).toBe(expectedEnabledState);
    }

    function waitForKendoGrid() {
        return CatalogHelper.waitForKendoGrid();
    }

    function loadPage() {
        console.log("Loading system catalog page");
        return pageObject.getPage();
    }

    function createSystemName() {
        return "System" + new Date().getTime();
    }

    function expectSystemWithName(name: string) {
        console.log("Making sure " + name + " does exist");
        return expect(findCatalogColumnsFor(name).first().getText()).toEqual(name);
    }

    function expectNoSystemWithName(name: string) {
        console.log("Making sure " + name + " does not exist");
        return expect(findCatalogColumnsFor(name)).toBeEmptyArray();
    }

    function toggleSystemInUse(name: string) {
        return element(by.xpath('//*/tbody/*/td/a[text()="' + name + '"]/parent::*/parent::*//button')).click();
    }

    function getToastElement() {
        return element(cssHelper.byDataElementType("notification-message-block"));
    }

    function getToastText() {
        //notification-message-block
        return getToastElement().getText();
    }
    
});