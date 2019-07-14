import NavigationBarWrapper = require("../object-wrappers/NavigationBarWrapper");

var navigationBar = new NavigationBarWrapper();
class NavigationBarHelper {

    public dropDownExpand(): void {
        navigationBar.dropDownMenu.dropDownElement.click();
    }

    public logout(): void {
        navigationBar.dropDownMenu.logOut.click();
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