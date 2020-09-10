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
    public kendoToolbarWrapper = new KendoToolbarWrapper();

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

    isCreateDpaAvailable(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.getCreateDpaDialog());
    }

    findSpecificDpaInNameColumn(name: string) { 
        console.log(`Finding dpa with name : ${name}`);
        return element(by.xpath(`//*/tbody/*/td/a[text()="${name}" and @data-element-type="kendo-dpa-name-rendering"]`));
    }
}
export = DataProcessingAgreementOverviewPageObject;
