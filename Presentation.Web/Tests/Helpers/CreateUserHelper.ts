import HomePageObjects = require("../PageObjects/Organization/UsersPage.po");
import CreatePage = require("../PageObjects/Organization/CreateUserPage.po");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import Login = require("../Helpers/LoginHelper");
import WaitTimers = require("../Utility/waitTimers");

var testFixture = new TestFixtureWrapper();
var pageCreateObject = new CreatePage();
var pageObject = new HomePageObjects();
var loginHelper = new Login();
var waitUpTo = new WaitTimers();
var ec = protractor.ExpectedConditions;

class CreateUserHelper {
    createrUserWithAPI(email: string, name: string, lastname: string, phoneNumber: string, API: boolean) {

        loginHelper.loginAsGlobalAdmin();
        pageObject.getPage();
        browser.wait(ec.presenceOf(pageObject.createUserButton), waitUpTo.twentySeconds);

        pageObject.createUserButton.click();

        pageCreateObject.inputEmail.sendKeys(email);

        pageCreateObject.inputEmailRepeat.sendKeys(email);

        pageCreateObject.inputName.sendKeys(name);

        pageCreateObject.inputLastName.sendKeys(lastname);

        pageCreateObject.inputPhone.sendKeys(phoneNumber);

        if (API) {
            pageCreateObject.boolApi.click();
        }

        pageCreateObject.createUserButton.click();
        browser.wait(ec.presenceOf(pageObject.createUserButton), waitUpTo.twentySeconds);
    }


    public deleteUser(email: string) {
        loginHelper.loginAsGlobalAdmin();
        pageObject.getPage();
        browser.wait(ec.presenceOf(pageObject.createUserButton), waitUpTo.twentySeconds);

        element.all(by.id("mainGrid")).all(by.tagName("tr")).each((ele) => {
            ele.all(by.tagName("td")).each((tdele) => {
                tdele.getText().then(val => {

                    if (val === email) {
                        ele.element(by.linkText("Slet")).click();
                        element(by.buttonText("Slet bruger")).click();

                    }
                });
            });
        });

    }

    public checkApiRoleStatusOnUser(email: string, apiStatus : boolean) {

        var res = null;

        element.all(by.id("mainGrid")).all(by.tagName("tr")).each((ele) => {
            ele.all(by.tagName("td")).each((tdele) => {
                tdele.getText().then(val => {
                    if (val === email) {
                        ele.element(by.linkText("Redigér")).click();
                        pageObject.hasAPiCheckBox.isSelected().then(selected => {
                            expect(selected).not.toBeNull();
                            expect(selected).toBe(apiStatus);
                            return;
                        });
                    }
                });
            });
        });
    }


    public updateApiOnUser(email: string, apiAccess: boolean, ) {
        loginHelper.loginAsGlobalAdmin();
        pageObject.getPage();
        browser.wait(ec.presenceOf(pageObject.createUserButton), waitUpTo.twentySeconds);

        element.all(by.id("mainGrid")).all(by.tagName("tr")).each((ele) => {
            ele.all(by.tagName("td")).each((tdele) => {
                tdele.getText().then(val => {
                    if (val === email) {
                        ele.element(by.linkText("Redigér")).click();
                        if (apiAccess) {
                           pageObject.hasAPiCheckBox.isSelected().then(selected => {
                                if (!selected) {
                                    pageCreateObject.boolApi.click();
                                    pageCreateObject.editUserButton.click();
                                    return;
                                } else {
                                    pageCreateObject.cancelEditUserButton.click();
                                    return;
                                }
                            });
                        }
                        else {
                            pageObject.hasAPiCheckBox.isSelected().then(selected => {
                                if (selected) {
                                    pageCreateObject.boolApi.click();
                                    pageCreateObject.editUserButton.click();
                                    return;
                                } else {
                                    pageCreateObject.cancelEditUserButton.click();
                                    return;
                                }
                            });
                        }

                    }
                });
            });
        });


    }

}
export = CreateUserHelper;