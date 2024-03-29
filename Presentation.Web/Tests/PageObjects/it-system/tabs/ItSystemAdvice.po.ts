﻿import IPageObject = require("../../IPageObject.po");
import KendoToolbarWrapper = require("../../../object-wrappers/KendoToolbarWrapper");
import NavigationHelper = require("../../../Utility/NavigationHelper");
import CssLocatorHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import KendoLoaderHelper = require("../../../Helpers/KendoLoaderHelper");
var ec = protractor.ExpectedConditions;

class ItSystemAdvice implements IPageObject {

    private navigationHelper = new NavigationHelper();
    private kendoLoaderHelper = new KendoLoaderHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();

    public getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/system/edit/1/advice");
    }
}

export = ItSystemAdvice;