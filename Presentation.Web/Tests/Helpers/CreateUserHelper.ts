import HomePage = require("../PageObjects/Organization/UsersPage.po");
import CreatePage = require("../PageObjects/Organization/CreateUserPage.po");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import Login = require("../Helpers/LoginHelper");
import WaitTimers = require("../Utility/waitTimers");

var testFixture = new TestFixtureWrapper();
var pageCreateObject = new CreatePage();
var pageObject = new HomePage();
var loginHelper = new Login();
var waitUpTo = new WaitTimers();
var ec = protractor.ExpectedConditions;

class CreateUserHelper {
    createrUserWithAPI(email: string, name: string, lastname: string, phoneNumber: string, API: boolean) {

        loginHelper.loginAsGlobalAdmin();
        pageObject.getPage();
        browser.wait(ec.presenceOf(element(by.linkText("Opret Bruger"))), waitUpTo.twentySeconds);

        element(by.linkText("Opret Bruger")).click();

        pageCreateObject.inputEmail.sendKeys(email);

        pageCreateObject.inputEmailRepeat.sendKeys(email);

        pageCreateObject.inputName.sendKeys(name);

        pageCreateObject.inputLastName.sendKeys(lastname);

        pageCreateObject.inputPhone.sendKeys(phoneNumber);

        if (API) {
            pageCreateObject.boolApi.click();
        }

        pageCreateObject.createUserButton.click();
        browser.wait(ec.presenceOf(element(by.linkText("Opret Bruger"))), waitUpTo.twentySeconds);
    }


    public deleteUser(email: string) {
        loginHelper.loginAsGlobalAdmin();
        pageObject.getPage();
        browser.wait(ec.presenceOf(element(by.linkText("Opret Bruger"))), waitUpTo.twentySeconds);

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
                        element(by.model("ctrl.vm.hasApi")).isSelected().then(selected => {
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
        browser.wait(ec.presenceOf(element(by.linkText("Opret Bruger"))), waitUpTo.twentySeconds);

        element.all(by.id("mainGrid")).all(by.tagName("tr")).each((ele) => {
            ele.all(by.tagName("td")).each((tdele) => {
                tdele.getText().then(val => {
                    if (val === email) {
                        ele.element(by.linkText("Redigér")).click();
                        if (apiAccess) {
                            element(by.model("ctrl.vm.hasApi")).isSelected().then(selected => {
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
                            element(by.model("ctrl.vm.hasApi")).isSelected().then(selected => {
                                console.debug("Checking checkbox API is : " + selected);
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