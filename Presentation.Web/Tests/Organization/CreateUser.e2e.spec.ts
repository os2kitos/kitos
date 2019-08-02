import HomePage = require("../PageObjects/Organization/UsersPage.po");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import Login = require("../Helpers/LoginHelper");
import WaitTimers = require("../Utility/waitTimers");
import createUserHelper = require("../Helpers/CreateUserHelper");

var testFixture = new TestFixtureWrapper();
var userHelper = new createUserHelper();
var pageObject = new HomePage();
var loginHelper = new Login();
var waitUpTo = new WaitTimers();
var ec = protractor.ExpectedConditions;

describe("Only Global Admins can create user with API access",
    () => {

        beforeAll(() => {

        });

        beforeEach(() => {

        });

        afterEach(() => {
            testFixture.cleanupState();
        });


        it("Global Admin can enable api access on new user",
            () => {
                loginHelper.loginAsGlobalAdmin();
                pageObject.getPage();
                browser.wait(ec.presenceOf(element(by.linkText("Opret Bruger"))), waitUpTo.twentySeconds);
                element(by.linkText("Opret Bruger")).click();
                expect(element(by.model("ctrl.vm.hasApi")).isDisplayed()).toBeTrue();
            });

        it("Local Admin cannot enable api access on new user",
            () => {
                loginHelper.loginAsLocalAdmin();
                pageObject.getPage();
                browser.wait(ec.presenceOf(element(by.linkText("Opret Bruger"))), waitUpTo.twentySeconds);
                element(by.linkText("Opret Bruger")).click();
                expect(element(by.model("ctrl.vm.hasApi")).isDisplayed()).toBeFalse();
            });


        it("Global admin is able to edit api access on existing user", () => {

                userHelper.updateApiOnUser("local-regular-user@kitos.dk", true);
                //Check på om brugern har fået API adgang ude i overview
                browser.wait(ec.presenceOf(pageObject.kendoToolbarWrapper.columnHeaders().userApi), waitUpTo.twentySeconds);
                expect(pageObject.kendoToolbarWrapper.columnHeaders().userApi.isDisplayed()).toBeTruthy();
                userHelper.checkApiRoleStatusOnUser("local-regular-user@kitos.dk",true);


            });
    });


