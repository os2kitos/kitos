import IPageObject = require("../IPageObject.po");
import NavigationBarHelper = require("../../Helpers/NavigationBarHelper")

class LoginPagePo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl);
    }

    public navigationBarHelper = new NavigationBarHelper();
    
}

export = LoginPagePo;