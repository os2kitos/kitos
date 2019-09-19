import IPageObject = require("../../IPageObject.po");
import KendoToolbarHelper = require("../../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../../object-wrappers/KendoToolbarWrapper")
import CssLocatorHelper = require("../../../object-wrappers/CSSLocatorHelper");
import Constants = require("../../../Utility/Constants");

class ItSystemCatalog implements IPageObject {
    private consts = new Constants();
    private ec = protractor.ExpectedConditions;
    private byDataElementType = new CssLocatorHelper().byDataElementType;

    public getPage(): webdriver.promise.Promise<void> {
        return browser.getCurrentUrl()
            .then(url => {
                const navigateToUrl = browser.baseUrl + "/#/system/catalog";
                if (navigateToUrl !== url) {
                    console.log("Not at " + navigateToUrl + " but at:" + url + ". Navigating to:" + navigateToUrl);
                    return browser.get(browser.baseUrl + "/#/system/catalog");
                } else {
                    console.log("Already at " + navigateToUrl + ". Ignoring command");
                }
            });
    }

    public kendoToolbarHelper = new KendoToolbarHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();
    public createCatalogForm = element(this.byDataElementType(this.consts.catalogCreateForm));

    public isCreateCatalogAvailable(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.createCatalogForm);
    }

    public waitForKendoGrid(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.kendoToolbarWrapper.columnObjects().catalogName.first());
    }
}

export = ItSystemCatalog;