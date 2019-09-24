import IPageObject = require("../../IPageObject.po");
import KendoToolbarHelper = require("../../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../../object-wrappers/KendoToolbarWrapper")
import CssLocatorHelper = require("../../../object-wrappers/CSSLocatorHelper");
import Constants = require("../../../Utility/Constants");
import NavigationHelper = require("../../../Utility/NavigationHelper");

class ItSystemCatalog implements IPageObject {
    private consts = new Constants();
    private ec = protractor.ExpectedConditions;
    private byDataElementType = new CssLocatorHelper().byDataElementType;
    private navigationHelper = new NavigationHelper();


    public getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/system/catalog");
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