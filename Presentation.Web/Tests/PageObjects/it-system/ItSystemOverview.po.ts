import IPageObject = require("../IPageObject.po");
import KendoToolbarHelper = require("../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper")

class ItSystemEditPo implements IPageObject { 

    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/system/overview");
    }

    public kendoToolbarHelper = new KendoToolbarHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();

}

export = ItSystemEditPo;