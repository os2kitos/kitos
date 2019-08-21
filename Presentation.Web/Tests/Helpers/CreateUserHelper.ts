import HomePageObjects = require("../PageObjects/Organization/UsersPage.po");
import CreatePage = require("../PageObjects/Organization/CreateUserPage.po");
import Login = require("../Helpers/LoginHelper");
import WaitTimers = require("../Utility/waitTimers");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");

var cssHelper = new CSSLocator();
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

    public checkApiRoleStatusOnUser(email: string, apiStatus: boolean) {

        var checked = false;
        pageObject.mainGridAllTableRows.each((row) => {
            //TODO: Run locally and fix it
            row.element(cssHelper.byDataElementType("userEmailObject")).getText().then(text => {
                if (text === email) {
                    row.element(by.linkText("Redigér")).click();
                    checked = true;
                    //TODO: Run locally and fix it
                    expect(pageObject.hasAPiCheckBox.getAttribute("checked")).toEqual(apiStatus);
                }
            });
        });
        expect(checked).toBe(true);
    }


    public updateApiOnUser(email: string, apiAccess: boolean) {

        loginHelper.loginAsGlobalAdmin();
        pageObject.getPage();
        browser.wait(ec.presenceOf(pageObject.createUserButton), waitUpTo.twentySeconds);
        pageObject.mainGridAllTableRows.each((row) => {
            row.element(cssHelper.byDataElementType("userEmailObject")).getText().then(val => {
                if (val === email) {
                    row.element(by.linkText("Redigér")).click();
                    if (apiAccess) {
                        //TODO: Run locally and fix it
                        //TODO: Migrate IsSelected
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
                        //TODO: Migrate IsSelected
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
    }
}
export = CreateUserHelper;