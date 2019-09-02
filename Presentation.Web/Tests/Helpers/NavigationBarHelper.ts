import NavigationBarWrapper = require("../object-wrappers/NavigationBarWrapper");

var navigationBar = new NavigationBarWrapper();
class NavigationBarHelper {

    public dropDownExpand() {
        return  navigationBar.dropDownMenu.dropDownElement.click();
    }

    public logout() {
        return navigationBar.dropDownMenu.logOut.click();
    }

    public isMyProfileDisplayed(): webdriver.promise.Promise<Boolean>{
        return navigationBar.dropDownMenu.myProfile.isPresent();
    }

    public isGlobalAdminDisplayed(): webdriver.promise.Promise<Boolean> {
        return navigationBar.dropDownMenu.globalAdmin.isPresent();
    }

    public isLocalAdminDisplayed(): webdriver.promise.Promise<Boolean> {
        return navigationBar.dropDownMenu.localAdmin.isPresent();
    }
    
}

export = NavigationBarHelper;