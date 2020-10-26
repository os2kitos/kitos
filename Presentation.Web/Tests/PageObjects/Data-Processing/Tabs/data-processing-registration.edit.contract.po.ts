import PageObject = require("../../IPageObject.po");
import NavigationHelper = require("../../../Utility/NavigationHelper");

class DataProcessingRegistrationEditContractPageObject {

    private navigationHelper = new NavigationHelper();

    refreshPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.refreshPage();
    }

    static getContractLink(contractName: string) {
        return element(by.linkText(contractName));
    }

}
export = DataProcessingRegistrationEditContractPageObject;
