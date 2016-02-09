import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");

class ItInterfaceEditPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/system/edit/1/interface-details");
    }

    // delete interface
    public deleteInterfaceElement = element.all(by.css("a.btn-danger .glyphicon-minus"));

    // name input
    public nameElement = element(by.css("#interface-name"));
    public nameInput(value: string) {
        this.nameElement.sendKeys(value);
    }

    // id input
    public idElement = element(by.css("#interface-itInterfaceId"));
    public idInput(value: string) {
        this.idElement.sendKeys(value);
    }

    // version input
    public versionElement = element(by.css("#interface-version"));
    public versionInput(value: string) {
        this.versionElement.sendKeys(value);
    }

    // belongs to input
    public belongsToSelect = new Select2Wrapper("#s2id_belongs-to");

    // accessModifier input
    public accessModifierSelect = new Select2Wrapper("#s2id_interface-access");

    // description input
    public descriptionElement = element(by.css("#interface-description"));
    public descriptionInput(value: string) {
        this.descriptionElement.sendKeys(value);
    }

    // url input
    public urlElement = element(by.css("#interface-url"));
    public urlInput(value: string) {
        this.urlElement.sendKeys(value);
    }

    // note input
    public noteElement = element(by.css("#interface-note"));
    public noteInput(value: string) {
        this.noteElement.sendKeys(value);
    }
}

export = ItInterfaceEditPo;
