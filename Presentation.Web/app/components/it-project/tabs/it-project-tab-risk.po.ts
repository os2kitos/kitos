import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");

class ItProjectEditTabRisk implements IPageObject {
    controllerVm = "";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/risk");
    }

    // risk repeater
    riskRepeater = new RepeaterWrapper(`risk in ${this.controllerVm}risks`);
    nameLocator = by.model("risk.name");
    consequenceLocator = by.model("risk.consequence");
    probabilityLocator = by.model("risk.probability");
    productLocator = by.css(".product");
    actionLocator = by.model("risk.action");
    responsibleLocator = by.css(".responsible");
    deleteRiskLocator = by.css(".delete-risk");

    // responsible selectbox of first row
    getResponsibleSelect(index: number): protractor.promise.Promise<Select2Wrapper> {
        return this.riskRepeater
            .select(index, this.responsibleLocator)
            .first()
            .getAttribute("id")
            .then(v => {
                return new Select2Wrapper(`#${v}`);
            });
    }

    // new risk name
    nameElement = element(by.model(this.controllerVm + "newRisk.name"));
    nameInput(value: string) {
        this.nameElement.sendKeys(value);
    }

    // new risk consequence
    consequenceElement = element(by.model(this.controllerVm + "newRisk.consequence"));
    consequenceInput(value: number) {
        this.consequenceElement.sendKeys(protractor.Key.CONTROL, "a", protractor.Key.NULL, value.toString()); // CTRL + a to clear old value
    }

    // new risk probability
    probabilityElement = element(by.model(this.controllerVm + "newRisk.probability"));
    probabilityInput(value: number) {
        this.probabilityElement.sendKeys(protractor.Key.CONTROL, "a", protractor.Key.NULL, value.toString()); // CTRL + a to clear old value
    }

    // new risk product
    productValue() {
        return element(by.css("#product")).getInnerHtml();
    }

    // new risk action
    actionElement = element(by.model(this.controllerVm + "newRisk.action"));
    actionInput(value: string) {
        this.actionElement.sendKeys(value);
    }

    // new risk responsible
    responsibleSelect = new Select2Wrapper("#s2id_responsible");

    // save new risk
    saveRiskElement = element(by.css("#save-risk"));

    // average risk product
    averageProductValue() {
        return element(by.css("#average-product")).getInnerHtml();
    }
}

export = ItProjectEditTabRisk;
