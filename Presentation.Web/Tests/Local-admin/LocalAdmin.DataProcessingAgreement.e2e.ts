import Login = require("../Helpers/LoginHelper");
import DPA = require("../PageObjects/Local-admin/LocalDataProcessingAgreement.po");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import NavigationBarHelper = require("../object-wrappers/navigationBarWrapper");

var checkboxBool;

describe("Local admin is able to toggle DataProcessingAgreement", () => {

    var loginHelper = new Login();
    var dpaPageHelper = new DPA();
    var testFixture = new TestFixtureWrapper();
    var naviHelper = new NavigationBarHelper();

    beforeAll(() => {
        loginHelper.loginAsLocalAdmin();
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("Option to toggle DataProcessingAgreement is visible", () => {
        dpaPageHelper.getPage();
        browser.waitForAngular();
        expect((dpaPageHelper.dpaCheckbox).isPresent()).toBe(true);
    });

    it("Is able to toggle DataProcessingAgreement checkbox", () => {
        dpaPageHelper.getPage().then(() => {
            browser.waitForAngular();
        }).then(() => {
            checkboxBool = dpaPageHelper.dpaCheckbox.isSelected();
            expect((dpaPageHelper.dpaCheckbox).isSelected()).toBe(checkboxBool);
        }).then(() => {
            return expect((naviHelper.headerNavigations.dataProcessingAgreementButton).isPresent()).toBe(checkboxBool);
        }).then(() => {
            dpaPageHelper.dpaCheckbox.click();
        }).then(() => {
            expect((dpaPageHelper.dpaCheckbox).isSelected()).toBe(!checkboxBool);
        }).then(() => {
            return expect((naviHelper.headerNavigations.dataProcessingAgreementButton).isPresent()).toBe(!checkboxBool);
        });

    });

});


