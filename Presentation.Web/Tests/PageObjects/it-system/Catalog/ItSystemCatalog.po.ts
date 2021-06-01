import IPageObject = require("../../IPageObject.po");
import KendoToolbarHelper = require("../../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../../object-wrappers/KendoToolbarWrapper")
import CssLocatorHelper = require("../../../object-wrappers/CSSLocatorHelper");
import Constants = require("../../../Utility/Constants");
import NavigationHelper = require("../../../Utility/NavigationHelper");
import KendoLoaderHelper = require("../../../Helpers/KendoLoaderHelper");

class ItSystemCatalog implements IPageObject {
    
    private consts = new Constants();
    private ec = protractor.ExpectedConditions;
    private byDataElementType = new CssLocatorHelper().byDataElementType;
    private navigationHelper = new NavigationHelper();
    private kendoLoaderHelper = new KendoLoaderHelper();

    getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/system/catalog");
    }

    kendoToolbarHelper = new KendoToolbarHelper();
    kendoToolbarWrapper = new KendoToolbarWrapper();
    createCatalogForm = element(this.byDataElementType(this.consts.catalogCreateForm));

    isCreateCatalogAvailable(): webdriver.until.Condition<boolean> {
        return this.ec.presenceOf(this.createCatalogForm);
    }

    isCreateCatalogVisible(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.createCatalogForm);
    }

    waitForKendoGrid(){
        return this.kendoLoaderHelper.waitForKendoGridData(this.kendoToolbarWrapper.columnObjects().catalogName.first());
    }

    getCreateSystemButton() {
        return this.kendoToolbarWrapper.headerButtons().systemCatalogCreate;
    }
}

export = ItSystemCatalog;