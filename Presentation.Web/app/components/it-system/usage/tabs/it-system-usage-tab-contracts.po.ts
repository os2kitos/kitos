import IPageObject = require("../../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../../Tests/object-wrappers/RepeaterWrapper");
import Select2Wrapper = require("../../../../../Tests/object-wrappers/Select2Wrapper");

class ItSystemUsageTabContractsPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/system/usage/1/contracts");
    }

    // contract repeater
    public contractsRepeater = new RepeaterWrapper("contract in usage.contracts");

    // contract selector
    public contractSelector = new Select2Wrapper("#s2id_contract-selector");
}

export = ItSystemUsageTabContractsPo;
