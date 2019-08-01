import HomePage = require("../PageObjects/Organization/UsersPage.po");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import Login = require("../Helpers/LoginHelper");
import WaitTimers = require("../Utility/waitTimers");

var testFixture = new TestFixtureWrapper();
var pageObject = new HomePage();
var loginHelper = new Login();
var waitUpTo = new WaitTimers();
var ec = protractor.ExpectedConditions;

describe("Only Global Admins can create user with API access", () => {

    beforeEach(() => {

    });

    afterEach(() => {
        testFixture.cleanupState();
    });

    it("Global Admin can enable api access on new user", () => {
        loginHelper.loginAsGlobalAdmin();
        pageObject.getPage();
        browser.wait(ec.presenceOf(element(by.linkText("Opret Bruger"))), waitUpTo.twentySeconds);
        element(by.linkText("Opret Bruger")).click();
        expect(element(by.model("ctrl.vm.hasApi")).isDisplayed()).toBeTrue();
    });

    it("Local Admin cannot enable api access on new user", () => {
        loginHelper.loginAsLocalAdmin();
        pageObject.getPage();
        browser.wait(ec.presenceOf(element(by.linkText("Opret Bruger"))), waitUpTo.twentySeconds);
        element(by.linkText("Opret Bruger")).click();
        expect(element(by.model("ctrl.vm.hasApi")).isDisplayed()).toBeFalse();
    });

});