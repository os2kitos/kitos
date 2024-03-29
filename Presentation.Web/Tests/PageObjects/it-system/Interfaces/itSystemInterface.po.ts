﻿import IPageObject = require("../../IPageObject.po");
import KendoToolbarWrapper = require("../../../object-wrappers/KendoToolbarWrapper");
import CssHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import NavigationHelper = require("../../../Utility/NavigationHelper");
import Constants = require("../../../Utility/Constants");
import KendoLoaderHelper = require("../../../Helpers/KendoLoaderHelper");

var ec = protractor.ExpectedConditions;

class ItSystemInterfaceCatalog implements IPageObject {
    private navigationHelper = new NavigationHelper();
    private constants = new Constants();
    private kendoLoaderHelper = new KendoLoaderHelper()
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

    public getInterfaceBelongsToField() {
        return element(this.cssHelper.byDataElementType(this.constants.interfaceBelongsToReadonly));
    }

    public waitForKendoGrid() {
        return this.kendoLoaderHelper.waitForKendoGridData(this.kendoToolbarWrapper.columnObjects().interfaceName.first());
    }
}
export = ItSystemInterfaceCatalog;