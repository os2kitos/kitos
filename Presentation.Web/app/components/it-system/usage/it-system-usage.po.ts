import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");

class ItSystemUsagePo implements IPageObject {
    public controllerVm = "";

    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/system/usage/1/interfaces");
    }

    // delete usage
    public deleteUsageElement = element.all(by.css("a.btn-danger .glyphicon-minus"));

    // localSystemId input
    public localSystemIdElement = element(by.css("#sysId"));
    public localSystemIdInput(value: string) {
        this.localSystemIdElement.sendKeys(value);
    }

    // localCallName input
    public localCallNameElement = element(by.css("#localcallname"));
    public localCallNameInput(value: string) {
        this.localCallNameElement.sendKeys(value);
    }

    // sensitiveData selector
    public sensitiveSelector = new Select2Wrapper("#s2id_sensitive");

    // esdh input
    public esdhElement = element(by.css("#esdh"));
    public esdhInput(value: string) {
        this.esdhElement.sendKeys(value);
    }

    // link input
    public linkElement = element(by.css("#url"));
    public linkInput(value: string) {
        this.linkElement.sendKeys(value);
    }

    // version input
    public versionElement = element(by.css("#version"));
    public versionInput(value: string) {
        this.versionElement.sendKeys(value);
    }

    // usageOwner input
    public usageOwnerElement = element(by.css("#usage-owner"));
    public usageOwnerInput(value: string) {
        this.usageOwnerElement.sendKeys(value);
    }

    // overview selector
    public overviewSelector = new Select2Wrapper("#s2id_overview");

    // archive selector
    public archiveSelector = new Select2Wrapper("#s2id_archive");

    // cmdb input
    public cmdbElement = element(by.css("#cmdb"));
    public cmdbInput(value: string) {
        this.cmdbElement.sendKeys(value);
    }

    // note input
    public noteElement = element(by.css("#note"));
    public noteInput(value: string) {
        this.noteElement.sendKeys(value);
    }
}

export = ItSystemUsagePo;
