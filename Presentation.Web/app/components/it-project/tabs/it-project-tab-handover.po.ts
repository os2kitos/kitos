import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2TagWrapper = require("../../../../Tests/object-wrappers/Select2TagWrapper");

class ItProjectEditTabHandover implements IPageObject {
    controllerVm: string = "";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/handover");
    }

    // desciption
    descriptionElement = element(by.css("#description"));
    descriptionInput(value: string) {
        this.descriptionElement.sendKeys(value);
    }

    // meeting
    meetingElement = element(by.css("#meeting"));
    meetingInput(value: string) {
        this.meetingElement.sendKeys(protractor.Key.CONTROL, "a", protractor.Key.NULL, value);
    }

    // participants
    participantsSelect = new Select2TagWrapper("#s2id_participants");

    // summary
    summaryElement = element(by.css("#summary"));
    summaryInput(value: string) {
        this.summaryElement.sendKeys(value);
    }
}

export = ItProjectEditTabHandover;
