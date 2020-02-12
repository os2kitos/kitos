﻿import IPageObject = require("../../IPageObject.po");
import KendoToolbarWrapper = require("../../../object-wrappers/KendoToolbarWrapper");
import CssHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import NavigationHelper = require("../../../Utility/NavigationHelper");

var ec = protractor.ExpectedConditions;

class ItSystemInterfaceCatalog implements IPageObject {
    private ec = protractor.ExpectedConditions;
    private navigationHelper = new NavigationHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();
    public cssHelper = new CssHelper();

    public getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/system/interface-catalog");
    }

    public isAlertVisible(): webdriver.until.Condition<boolean> {
        return ec.alertIsPresent();
    }

    public getCreateInterfaceButton() {
        return element(this.cssHelper.byDataElementType("createInterfaceButton"));
    }

    public getSaveInterfaceButton() {
        return element(this.cssHelper.byDataElementType("interfaceSaveButton"));
    }

    public getInterfaceNameInputField() {
        return element(this.cssHelper.byDataElementType("interfaceNameInput"));
    }

    public waitForKendoGrid(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.kendoToolbarWrapper.columnObjects().catalogName.first());
    }
}
export = ItSystemInterfaceCatalog;