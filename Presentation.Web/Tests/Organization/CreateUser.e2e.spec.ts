﻿import HomePage = require("../PageObjects/Organization/UsersPage.po");
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


        //it("Global admin is able to edit api access on existing user", () => {
        //    const credentials = loginHelper.getApiUserCredentials();
        //    console.log("Logging in");
        //    loginHelper.loginAsGlobalAdmin();

        //    console.log("Navigating to user page");
        //    pageObject.getPage();
        //    browser.wait(ec.presenceOf(pageObject.createUserButton), waitUpTo.twentySeconds);

        //    console.log("Updating API status");
        //    userHelper.updateApiOnUser(credentials.username, true);

        //    browser.wait(ec.presenceOf(pageObject.kendoToolbarWrapper.columnHeaders().userApi), waitUpTo.twentySeconds);
        //    expect(pageObject.kendoToolbarWrapper.columnHeaders().userApi.isDisplayed()).toBeTruthy();

        //    console.log("Checking that status is updated");
        //    userHelper.checkApiRoleStatusOnUser(credentials.username, true);
        //});
    });


