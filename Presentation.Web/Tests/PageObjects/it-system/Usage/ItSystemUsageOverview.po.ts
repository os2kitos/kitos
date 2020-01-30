import IPageObject = require("../../IPageObject.po");
import NavigationHelper = require("../../../Utility/NavigationHelper")

class ItSystemOverview implements IPageObject {
    private navigationHelper = new NavigationHelper();

    public getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/system/overview");
    }

}

export = ItSystemOverview;