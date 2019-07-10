import IPageObject = require("../IPageObject.po");
import NavigationBarWrapper = require("../../object-wrappers/navigationBarWrapper")

class HomePagePo implements IPageObject {
    getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl);
    }

    emailField = element(by.model("email"));
    pwdField = element(by.model("password"));
    loginButton = element(by.buttonText('Log ind'));

    navigationDropdownMenu = new NavigationBarWrapper().dropDown;

}

export = HomePagePo;