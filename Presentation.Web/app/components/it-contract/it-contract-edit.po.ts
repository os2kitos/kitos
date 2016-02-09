import IPageObject = require("../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../Tests/object-wrappers/Select2Wrapper");
import Select2TagWrapper = require("../../../Tests/object-wrappers/Select2TagWrapper");

class ItContractEditPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/contract/edit/1/systems");
    }

    // delete contract
    public deleteContractButton = element.all(by.css("a.btn-danger .glyphicon-minus"));

    // name input
    public nameElement = element(by.css("#contract-name"));
    public nameInput = (value: string) => {
        this.nameElement.sendKeys(value);
    }

    // id input
    public idElement = element(by.css("#contract-id"));
    public idInput = (value: string) => {
        this.idElement.sendKeys(value);
    }

    // parent selector
    public parentSelector = new Select2Wrapper("#s2id_contract-parent");

    // type selector
    public typeSelector = new Select2Wrapper("#s2id_contract-type");

    // esdh input
    public esdhElement = element(by.css("#contract-esdh"));
    public esdhInput = (value: string) => {
        this.esdhElement.sendKeys(value);
    }

    // object owner input
    public objectOwnerElement = element(by.css("#object-owner"));
    public objectOwnerInput = (value: string) => {
        this.objectOwnerElement.sendKeys(value);
    }

    // purchaseform selector
    public purchaseformSelector = new Select2Wrapper("#s2id_contract-purchaseform");

    // strategy selector
    public strategySelector = new Select2Wrapper("#s2id_contract-strat");

    // plan selector
    public planSelector = new Select2Wrapper("#s2id_contract-plan");

    // template selector
    public templateSelector = new Select2Wrapper("#s2id_contract-template");

    // folder input
    public folderElement = element(by.css("#contract-folder"));
    public folderInput = (value: string) => {
        this.folderElement.sendKeys(value);
    }

    // note input
    public noteElement = element(by.css("#contract-note"));
    public noteInput = (value: string) => {
        this.noteElement.sendKeys(value);
    }

    // supplier selector
    public supplierSelector = new Select2Wrapper("#s2id_contract-supplier");

    // externSigner input
    public externSignerElement = element(by.css("#contract-ext-signer"));
    public externSignerInput = (value: string) => {
        this.externSignerElement.sendKeys(value);
    }

    // orgUnit selector
    public orgUnitSelector = new Select2Wrapper("#contract-orgunit .select2-container");

    // internSigner selector
    public internSignerSelector = new Select2Wrapper("#contract-int-signer .select2-container");

    // externSign checkbox
    public externSignCheckbox = element(by.css("#contract-ext-signed"));

    // externSignDate input
    public externSignDateElement = element(by.css("#contract-ext-date"));
    public externSignDateInput = (value: string) => {
        this.externSignDateElement.sendKeys(value);
    }

    // internSign checkbox
    public internSignCheckbox = element(by.css("#contract-int-signed"));

    // internSignDate input
    public internSignDateElement = element(by.css("#contract-int-date"));
    public internSignDateInput = (value: string) => {
        this.internSignDateElement.sendKeys(value);
    }

    // agreementElements selector
    public agreementElementsSelector = new Select2TagWrapper("#s2id_agreement-elements");
}

export = ItContractEditPo;
