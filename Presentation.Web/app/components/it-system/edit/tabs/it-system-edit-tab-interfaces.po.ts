import IPageObject = require("../../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../../Tests/object-wrappers/Select2Wrapper");
import RepeaterWrapper = require("../../../../../Tests/object-wrappers/RepeaterWrapper");

class ItSystemEditTabInterfacesPo implements IPageObject {
    public controllerVm = "";

    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/system/edit/1/interfaces");
    }

    // select interface
    public selectInterface = new Select2Wrapper("#s2id_select-interface");

    // interface repeater
    public interfaceRepeater = new RepeaterWrapper("interface in canUseInterfaces");
    public deleteInterfaceLocator = by.css(".delete-interface");
}

export = ItSystemEditTabInterfacesPo;
