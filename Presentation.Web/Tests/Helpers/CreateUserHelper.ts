import HomePageObjects = require("../PageObjects/Organization/UsersPage.po");
import CreatePage = require("../PageObjects/Organization/CreateUserPage.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");

class CreateUserHelper {
    private cssHelper = new CSSLocator();
    private pageCreateObject = new CreatePage();
    private pageObject = new HomePageObjects();
    public checkApiRoleStatusOnUser(email: string, apiStatus: boolean) {
        return this.openEditUser(email)
            .then(() => {
                var expectedValue = apiStatus ? "true" : null;
                return expect(this.pageObject.hasAPiCheckBox.getAttribute("checked")).toEqual(expectedValue);
            });
    }

    public updateApiOnUser(email: string, apiAccess: boolean) {
        return this.openEditUser(email)
            .then(() => {
                return this.pageObject.hasAPiCheckBox.isSelected()
                    .then(selected => {
                        if (selected !== apiAccess) {
                            return this.pageCreateObject.boolApi.click()
                                .then(() => {
                                   return this.pageCreateObject.editUserButton.click();
                                });
                        } else {
                            return this.pageCreateObject.cancelEditUserButton.click();
                        }
                    });
            });
    }

    private getUserRow(email: string) {
        const emailColumnElementType = "userEmailObject";

        var rows = this.pageObject.mainGridAllTableRows.filter((row, index) => {
            console.log("Searching for email column");
            var column = row.element(this.cssHelper.byDataElementType(emailColumnElementType));
            return column.isPresent()
                .then(present => {
                    if (present) {
                        console.log("Found email column - checking if row is the right one");
                        return column.getText()
                            .then(text => {
                                return text === email;
                            });
                    }
                    return false;
                });
        });

        return rows.first();
    }

    private openEditUser(email: string) {
        const row = this.getUserRow(email);
        expect(row).not.toBe(null);
        return row.element(by.linkText("Redigér")).click();
    }
}
export = CreateUserHelper;