import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

class ItProjectEditTabStakeholders implements IPageObject {
    controllerVm = "";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/stakeholders");
    }

    // stakeholder repeater
    stakeholderRepeater = new RepeaterWrapper(`stakeholder in ${this.controllerVm}stakeholders`);
    nameLocator = by.model("stakeholder.name");
    roleLocator = by.model("stakeholder.role");
    downsidesLocator = by.model("stakeholder.downsides");
    benefitsLocator = by.model("stakeholder.benefits");
    significanceLocator = by.model("stakeholder.significance");
    howToHandleLocator = by.model("stakeholder.howToHandle");
    deleteStakeholderLocator = by.css(".delete-stakeholder");

    // new stakeholder name
    nameElement = element(by.model(this.controllerVm + "new.name"));
    nameInput(value: string) {
        this.nameElement.sendKeys(value);
    }

    // new stakeholder role
    roleElement = element(by.model(this.controllerVm + "new.role"));
    roleInput(value: string) {
        this.roleElement.sendKeys(value);
    }

    // new stakeholder downsides
    downsidesElement = element(by.model(this.controllerVm + "new.downsides"));
    downsidesInput(value: string) {
        this.downsidesElement.sendKeys(value);
    }

    // new stakeholder benefits
    benefitsElement = element(by.model(this.controllerVm + "new.benefits"));
    benefitsInput(value: string) {
        this.benefitsElement.sendKeys(value);
    }

    // new stakeholder significance
    significanceElement = element(by.model(this.controllerVm + "new.significance"));
    significanceInput(value: number) {
        this.significanceElement.sendKeys(value.toString());
    }

    // new stakeholder howToHandle
    howToHandleElement = element(by.model(this.controllerVm + "new.howToHandle"));
    howToHandleInput(value: string) {
        this.howToHandleElement.sendKeys(value);
    }

    // save new stakeholder
    saveStakeholderElement = element(by.css("#save-stakeholder"));
}

export = ItProjectEditTabStakeholders;
