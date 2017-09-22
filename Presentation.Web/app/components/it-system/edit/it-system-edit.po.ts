import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");

class ItSystemEditPo implements IPageObject {
    public controllerVm = "";

    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/system/edit/1/interfaces");
    }

    // delete system
    public deleteSystemElement = element(by.css("a.btn-danger"));

    // appTypeOption
    public appTypeSelect = new Select2Wrapper("#s2id_system-type");

    // name
    public nameElement = element(by.model(this.controllerVm + "system.name"));
    public nameInput(value: string) {
        this.nameElement.sendKeys(value, protractor.Key.TAB);
    }

    // system parent
    public systemParentSelect = new Select2Wrapper("#s2id_system-parent");

    // belongs to
    public belongsToSelect = new Select2Wrapper("#s2id_system-belongs-to");

    // accessModifier input
    public accessModifierSelect = new Select2Wrapper("#s2id_system-access");

    // usageType selector
    public usageTypeSelector = new Select2Wrapper("#s2id_system-business-type");

    // description input
    public descriptionElement = element(by.css("#system-description"));
    public descriptionInput(value: string) {
        this.descriptionElement.sendKeys(value, protractor.Key.TAB);
    }

    // furtherDescription input
    public furtherDescriptionElement = element(by.css("#system-url"));
    public furtherDescriptionInput(value: string) {
        this.furtherDescriptionElement.sendKeys(value, protractor.Key.TAB);
    }
}

export = ItSystemEditPo;
