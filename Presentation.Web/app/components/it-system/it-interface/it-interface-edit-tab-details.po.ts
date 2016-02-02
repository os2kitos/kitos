import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

class ItInterfaceEditTabDetailsPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/system/edit/1/interface-details");
    }

    public tsaSelector = new Select2Wrapper("#s2id_interface-tsa");
    public typeSelector = new Select2Wrapper("#s2id_interface-type");
    public interfaceSelector = new Select2Wrapper("#s2id_interface-interface");
    public methodSelector = new Select2Wrapper("#s2id_interface-method");
    public exhibitSelector = new Select2Wrapper("#s2id_interface-exposed-by");

    public dataRowRepeater = new RepeaterWrapper("dataRow in dataRows");
    public dataLocator = by.css("[data-field=\"data\"]");
    public dataTypeSelector = new Select2Wrapper(".data-type-selector .select2-container");
    public deleteLocator = by.css(".delete-data-row");

    public addDataButton = element(by.css("#add-data-row"));
}

export = ItInterfaceEditTabDetailsPo;
