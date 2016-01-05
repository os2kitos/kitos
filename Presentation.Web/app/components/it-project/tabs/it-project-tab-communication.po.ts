import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");

class ItProjectEditTabCommunication implements IPageObject {
    controllerVm: string = "";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/communication");
    }

    // comm repeater
    commRepeater = new RepeaterWrapper("c in " + this.controllerVm + "comms");
    targetLocator = by.model("c.targetAudiance");
    purposeLocator = by.model("c.purpose");
    messageLocator = by.model("c.message");
    mediaLocator = by.model("c.media");
    dueDateLocator = by.model("c.dueDate");
    responsibleLocator = by.css(".responsible");
    deleteCommLocator = by.css(".delete-comm");

    // responsible selectbox of row in repeater
    getResponsibleSelect(index: number): protractor.promise.Promise<Select2Wrapper> {
        return this.commRepeater
            .select(index, this.responsibleLocator)
            .first()
            .getAttribute("id")
            .then(v => {
                return new Select2Wrapper("#" + v);
            });
    }

    // new comm target
    targetElement = element(by.model(this.controllerVm + "comm.targetAudiance"));
    targetInput(value: string) {
        this.targetElement.sendKeys(value);
    }

    // new comm purpose
    purposeElement = element(by.model(this.controllerVm + "comm.purpose"));
    purposeInput(value: string) {
        this.purposeElement.sendKeys(value);
    }

    // new comm message
    messageElement = element(by.model(this.controllerVm + "comm.message"));
    messageInput(value: string) {
        this.messageElement.sendKeys(value);
    }

    // new comm media
    mediaElement = element(by.model(this.controllerVm + "comm.media"));
    mediaInput(value: string) {
        this.mediaElement.sendKeys(value);
    }

    // new comm duedate
    dueDateElement = element(by.model(this.controllerVm + "comm.dueDate"));
    dueDateInput(value: string) {
        this.dueDateElement.sendKeys(value);
    }

    // new comm responsible
    responsibleSelect = new Select2Wrapper("#s2id_responsible");

    // save new comm
    saveCommElement = element(by.css("#save-comm"));
}

export = ItProjectEditTabCommunication;
