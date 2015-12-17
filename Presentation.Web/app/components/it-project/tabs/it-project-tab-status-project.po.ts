import IPageObject = require("../../../IPageObject.po");
import SelectStatus2Wrapper = require("../../../SelectStatus2Wrapper");

class ItPojectEditPo implements IPageObject {
    controllerVm: string = "projectStatusVm";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/status-project");
    }

    // status traffic light
    statusTrafficLightSelect = new SelectStatus2Wrapper("#statusProject");

    // date for status update
    statusUpdateDateElement = element(by.model(this.controllerVm + ".project.statusDate"));
    get statusUpdateInput(): string {
        var value: string;
        this.statusUpdateDateElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set statusUpdateInput(value: string) {
        this.statusUpdateDateElement.sendKeys(value);
    }

    // note for status
    statusNoteElement = element(by.model(this.controllerVm + ".project.statusNote"));
    get statusNoteInput(): string {
        var value: string;
        this.statusNoteElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set statusNoteInput(value: string) {
        this.statusNoteElement.sendKeys(value);
    }
}

export = ItPojectEditPo;
