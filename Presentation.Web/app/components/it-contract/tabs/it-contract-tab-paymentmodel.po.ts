import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

class ItContractEditTabPaymentModelPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/contract/edit/1/paymentmodel");
    }

    // agreementOperation input
    public agreementOperationElement = element(by.css("#agreement-operation"));
    public agreementOperationInput = (value: string) => {
        this.agreementOperationElement.sendKeys(value);
    }
}

export = ItContractEditTabPaymentModelPo;
