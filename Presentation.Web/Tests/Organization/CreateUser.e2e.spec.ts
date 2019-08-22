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

        afterEach(() => {
            testFixture.cleanupState();
        });


        it("Global Admin can enable api access on new user", () => {
                loginHelper.loginAsGlobalAdmin()
                    .then(() => {
                        pageObject.getPage();
                    })
                    .then(() => {
                        browser.wait(ec.presenceOf(pageObject.createUserButton), waitUpTo.twentySeconds);
                    })
                    .then(() => {
                        pageObject.createUserButton.click();
                    })
                    .then(() => {
                        expect(pageObject.hasAPiCheckBox.isDisplayed()).toBeTrue();
                    });
        });

        it("Local Admin cannot enable api access on new user", () => {
            loginHelper.loginAsLocalAdmin()
                .then(() => {
                    pageObject.getPage();
                })
                .then(() => {
                    browser.wait(ec.presenceOf(pageObject.createUserButton), waitUpTo.twentySeconds);
                })
                .then(() => {
                    pageObject.createUserButton.click();
                })
                .then(() => {
                    expect(pageObject.hasAPiCheckBox.isDisplayed()).toBeFalse();
                });
            });

        function canSetApiAccessTo(value: boolean) {
            const credentials = loginHelper.getLocalAdminCredentials();
            return pageObject.getPage()
                .then(() => {
                    browser.wait(ec.presenceOf(pageObject.createUserButton), waitUpTo.twentySeconds);
                })
                .then(() => {
                    console.log("Updating API status to " + value);
                    userHelper.updateApiOnUser(credentials.username, value);
                }).then(() => {
                    browser.wait(ec.presenceOf(pageObject.kendoToolbarWrapper.columnHeaders().userApi),
                        waitUpTo.twentySeconds);
                })
                .then(() => {
                    expect(pageObject.kendoToolbarWrapper.columnHeaders().userApi.isDisplayed()).toBeTruthy();

                    console.log("Checking that status is updated");
                    userHelper.checkApiRoleStatusOnUser(credentials.username, value);
                });
        }

        it("Global admin is able to set api access to TRUE on existing user", () => {
            canSetApiAccessTo(true);
        });

        it("Global admin is able to set api access to FALSE on existing user", () => {
            canSetApiAccessTo(false);
        });
    });


