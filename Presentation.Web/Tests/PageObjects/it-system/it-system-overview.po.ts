import IPageObject = require("../../object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../object-wrappers/Select2Wrapper");

class ItSystemEditPo implements IPageObject {

    getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/system/overview");
    }

    public resetFiltersButton = element(by.buttonText("Nulstil"));

    public saveFiltersButton = element(by.buttonText("Gem filter"));

    public useFiltersButton = element(by.buttonText("Anvend filter"));

    public deleteFiltersButton = element(by.buttonText("Slet filter"));

    public systemRoleSelect = new Select2Wrapper("#s2id_system-type");

    public exportButton = element(by.className("k-button k-grid-excel"))

}

export = ItSystemEditPo;