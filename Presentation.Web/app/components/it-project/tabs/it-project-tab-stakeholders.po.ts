import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

class ItProjectEditTabStakeholders implements IPageObject {
    controllerVm: string = "";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/stakeholders");
    }

    // stakeholder repeater
    stakeholderRepeater = new RepeaterWrapper("stakeholder in " + this.controllerVm + "stakeholders");
    nameLocator = by.model("stakeholder.name");
    roleLocator = by.model("stakeholder.role");
    downsidesLocator = by.model("stakeholder.downsides");
    benefitsLocator = by.model("stakeholder.benefits");
    significanceLocator = by.model("stakeholder.significance");
    howToHandleLocator = by.model("stakeholder.howToHandle");
    deleteStakeholderLocator = by.css(".delete-stakeholder");

    // new stakeholder name
    nameElement = element(by.model(this.controllerVm + "new.name"));
    get nameInput(): string {
        var value: string;
        this.nameElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set nameInput(value: string) {
        this.nameElement.sendKeys(value);
    }

    // new stakeholder role
    roleElement = element(by.model(this.controllerVm + "new.role"));
    get roleInput(): string {
        var value: string;
        this.roleElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set roleInput(value: string) {
        this.roleElement.sendKeys(value);
    }

    // new stakeholder downsides
    downsidesElement = element(by.model(this.controllerVm + "new.downsides"));
    get downsidesInput(): string {
        var value: string;
        this.downsidesElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set downsidesInput(value: string) {
        this.downsidesElement.sendKeys(value);
    }

    // new stakeholder benefits
    benefitsElement = element(by.model(this.controllerVm + "new.benefits"));
    get benefitsInput(): string {
        var value: string;
        this.benefitsElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set benefitsInput(value: string) {
        this.benefitsElement.sendKeys(value);
    }

    // new stakeholder significance
    significanceElement = element(by.model(this.controllerVm + "new.significance"));
    get significanceInput(): number {
        var value: string;
        this.significanceElement.getAttribute("value").then(v => value = v);
        return parseInt(value);
    }

    set significanceInput(value: number) {
        this.significanceElement.sendKeys(value.toString());
    }

    // new stakeholder howToHandle
    howToHandleElement = element(by.model(this.controllerVm + "new.howToHandle"));
    get howToHandleInput(): string {
        var value: string;
        this.howToHandleElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set howToHandleInput(value: string) {
        this.howToHandleElement.sendKeys(value);
    }

    // save new stakeholder
    saveStakeholderElement = element(by.css("#save-stakeholder"));
}

export = ItProjectEditTabStakeholders;
