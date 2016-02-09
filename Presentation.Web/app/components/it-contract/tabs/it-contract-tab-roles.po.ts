import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");

class ItContractEditTabRolesPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/contract/edit/1/roles");
    }

    // editContractSigner button
    public editContractSignerButton = element(by.css("#edit-contract-signer"));

    // contractSigner selector
    public contractSignerSelector = new Select2Wrapper("#contract-signer-user .select2-container");

    // saveContractoSigner element
    public saveContractoSignerElement = element(by.css("#save-contract-signer"));

    // rights repeater
    public rightsRepeater = new RepeaterWrapper("right in rights");
    public rightRowLocator = by.css("tr");
    public rightEditButtonLocator = by.css(".edit-right");
    public rightDeleteLocator = by.css(".delete-right");
    public rightEditRoleInputLocator = by.css(".edit-right-user");
    public rightEditSaveButtonLocator = by.css(".edit-right-save");

    // add right role selector
    public addRightRoleSelector = new Select2Wrapper("#s2id_add-right-role");

    // add right user selector
    public addRightUserSelector = new Select2Wrapper("#add-right-user .select2-container");
}

export = ItContractEditTabRolesPo;
