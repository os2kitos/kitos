import NavigationHelper = require("../../Utility/NavigationHelper");
import PageObject = require("../IPageObject.po");
import KendoToolbarWrapper = require("../../Object-wrappers/KendoToolbarWrapper");
import KendoLoaderHelper = require("../../Helpers/KendoLoaderHelper");
import CssLocatorHelper = require("../../Object-wrappers/CSSLocatorHelper");
import Constants = require("../../Utility/Constants");

class DataProcessingAgreementOverviewPageObject implements PageObject {
    private kendoLoaderHelper = new KendoLoaderHelper();
    private navigationHelper = new NavigationHelper();
    private ec = protractor.ExpectedConditions;
    private cssHelper = new CssLocatorHelper();
    private consts = new Constants();
    private kendoToolbarWrapper = new KendoToolbarWrapper();

    getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/data-processing/overview");
    }

    refreshPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.refreshPage();
    }

    waitForKendoGrid() {
        return this.kendoLoaderHelper.waitForKendoGridData(this.kendoToolbarWrapper.columnObjects().dpaName.first());
    }

    getCreateDpaButton() {
        return this.kendoToolbarWrapper.headerButtons().createDpa;
    }

    getNewDpaNameInput() {
        return element(this.cssHelper.byDataElementType("createDpaForm-input_name"));
    }

    getNewDpaSubmitButton() {
        return element(this.cssHelper.byDataElementType("createDpaForm-control_submit"));
    }

    getCreateDpaDialog() {
        return element(this.cssHelper.byDataElementType(this.consts.createDpaForm));
    }

    getRoleRow(roleName: string, userName: string): webdriver.promise.Promise<protractor.ElementFinder> {
        return element
            .all(by.id("table-of-assigned-roles"))
            .filter((row, index) => {
                var rowElements = row.element(by.tagName("tr")).all(by.tagName("td"));
                var roleNameCorrect = rowElements.get(0).getText().then(text => roleName === text);
                var userNameCorrect = rowElements.get(1).getText().then(text => userName === text);
                return roleNameCorrect && userNameCorrect;
            })
            .then(rows => {
                if (rows.length > 1) {
                    throw new Error(`Found more than 1 item with role: ${roleName} and user: ${userName}`);
                }
                if (rows.length < 1) {
                    throw new Error(`Found no items with role: ${roleName} and user: ${userName}`);
                }
                return rows[0];
            });
        
    }

    getRoleDeleteButton(roleName: string, userName: string) {
        return this.getRoleButtons(roleName, userName).then(row => row[1]);
    }

    getRoleEditButton(roleName: string, userName: string) {
        return this.getRoleButtons(roleName, userName).then(row => row[0]);
    }

    private getRoleButtons(roleName: string, userName: string) {
        return this.getRoleRow(roleName, userName)
            .then(row => row.all(by.tagName("td"))
                .get(2)
                .element(by.tagName("div"))
                .all(by.tagName("a")));
    }

    getRoleSubmitEditButton() {
        return element(by.id("submit-edit"));
    }

    getSystemRow(systemName: string) {
        return element(by.xpath(`//*/a[text()="${systemName}"]/../..`));
    }

    getRemoveSystemButton(systemName: string) {
        return element(by.xpath(`//*/a[text()="${systemName}"]/../..//button`));
    }

    isCreateDpaAvailable(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.getCreateDpaDialog());
    }

    findSpecificDpaInNameColumn(name: string) {
        console.log(`Finding dpa with name : ${name}`);
        return element(by.xpath(`//*/tbody/*/td/a[text()="${name}" and @data-element-type="kendo-dpa-name-rendering"]`));
    }
}
export = DataProcessingAgreementOverviewPageObject;
