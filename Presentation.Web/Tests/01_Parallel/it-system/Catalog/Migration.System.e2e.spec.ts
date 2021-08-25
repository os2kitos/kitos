import Login = require("../../../Helpers/LoginHelper");
import SystemCatalogHelper = require("../../../Helpers/SystemCatalogHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import Constants = require("../../../Utility/Constants");
import CssHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import CatalogPage = require("../../../PageObjects/it-system/Catalog/ItSystemCatalog.po");

describe("Global Administrator is able to migrate from one system to another", () => {
    var loginHelper = new Login();
    var testFixture = new TestFixtureWrapper();
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

    it("Local admin is not able to see the move button", () => {
        loginHelper.loginAsLocalAdmin()
            .then(() => pageObject.getPage())
            .then(() => SystemCatalogHelper.waitForKendoGrid())
            .then(() => openMigrationOnSpecificSystem("DefaultTestItSystem"))
            .then(() => expect(element(cssHelper.byDataElementType(constants.moveSystemButton)).isPresent()).toBe(false));
    });

    it("Regular user is not able to see the move button", () => {
        loginHelper.loginAsRegularUser()
            .then(() => pageObject.getPage())
            .then(() => SystemCatalogHelper.waitForKendoGrid())
            .then(() => openMigrationOnSpecificSystem("DefaultTestItSystem"))
            .then(() => expect(element(cssHelper.byDataElementType(constants.moveSystemButton)).isPresent()).toBe(false));
    });

    function openMigrationOnSpecificSystem(name: string) {
        console.log(`openMigrationOnSpecificSystem: ${name}`);
        return pageObject.getPage()
            .then(() => element(by.xpath('//*/tbody/*/td/a[text()="' + name + '"]/parent::*/parent::*//*/a[@data-element-type="usagesLinkText"]')).click());
    }
});