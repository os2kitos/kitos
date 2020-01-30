import IPageObject = require("../IPageObject.po");
import NavigationBarHelper = require("../../Helpers/NavigationBarHelper");
import NavigationBar = require("../../object-wrappers/NavigationBarWrapper");
import NavigationHelper = require("../../Utility/NavigationHelper");

class LoginPagePo implements IPageObject {

    private navigationHelper = new NavigationHelper();

    public getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("");
    }

    public navigationBarHelper = new NavigationBarHelper();

    public navigationBar = new NavigationBar();
}

export = LoginPagePo;