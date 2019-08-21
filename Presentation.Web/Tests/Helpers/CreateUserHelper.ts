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

        this.openEditUser(email);

        expect(pageObject.hasAPiCheckBox.getAttribute("checked")).toEqual(apiStatus);
    }

    public updateApiOnUser(email: string, apiAccess: boolean) {
        //TODO: The issue is the use of promises which are not resolved so these tests are unreliable
        this.openEditUser(email);

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

    private getUserRow(email: string) {
        const emailColumnElementType = "userEmailObject";

        //TODO: Rewrite to a promise and use the promise chain

        var rows = pageObject.mainGridAllTableRows.filter((row, index) => {
            if (row.element(cssHelper.byDataElementType(emailColumnElementType)).isPresent()) {
                var column = row.element(cssHelper.byDataElementType(emailColumnElementType));
                
            }
            
            //if (row.isElementPresent(cssHelper.byDataElementType(emailColumnElementType)).) {
            //    row.element(cssHelper.byDataElementType(emailColumnElementType)).getText().then(text => {
            //        return text === email;
            //    });
            //}
            return false;
        });

        return rows.first();
    }

    private openEditUser(email: string) {
        const row = this.getUserRow(email);
        expect(row).not.toBe(null);
        row.element(by.linkText("Redigér")).click();
    }
}
export = CreateUserHelper;