import IPageObject = require("../IPageObject.po");
import NavigationBarHelper = require("../../Helpers/NavigationBarHelper");
import NavigationBar = require("../../object-wrappers/NavigationBarWrapper");

class LoginPagePo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl);
    }

    public navigationBarHelper = new NavigationBarHelper();

    public navigationBar = new NavigationBar();
}

export = LoginPagePo;