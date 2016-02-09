import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");

class ItContractEditTabPaymentModelPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/contract/edit/1/paymentmodel");
    }

    // operationDate input
    public operationDateElement = element(by.css("#agreement-operation"));
    public operationDateInput = (value: string) => {
        this.operationDateElement.sendKeys(value);
    }

    // freq selector
    public freqSelector = new Select2Wrapper("#s2id_agreement-freq");

    // payment selector
    public paymentSelector = new Select2Wrapper("#s2id_agreement-payment");

    // price selector
    public priceSelector = new Select2Wrapper("#s2id_agreement-price");
}

export = ItContractEditTabPaymentModelPo;
