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

    it("Correct error message when trying to delete system in use", () => {
        var systemName = createSystemName();
        var testFixture = new TestFixtureWrapper();

        loginHelper.loginAsGlobalAdmin()
            .then(() => loadPage())
            .then(() => waitForKendoGrid())
            .then(() => expectCreateButtonVisibility(true))
            .then(() => CatalogHelper.createSystem(systemName))
            .then(() => console.log("Expecting systme with name " + systemName))
            .then(() => expectSystemWithName(systemName))
            .then(() => console.log("Toggling system " + systemName))
            .then(() => toggleSystemInUse(systemName))
            .then(() => console.log("Trying to delete IT-System"))
            .then(() => CatalogHelper.deleteSystemWithoutBrowserWait(systemName))
            .then(() => console.log("Waitng for toast message"))
            .then(() => browser.wait(getToastElement().isPresent(), 20000))
            .then(() => expect(getToastText()).toEqual("Fejl! Kunne ikke slette IT System!"))
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