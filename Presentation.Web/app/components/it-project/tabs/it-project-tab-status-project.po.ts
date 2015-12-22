import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import SelectStatus2Wrapper = require("../../../../Tests/object-wrappers/SelectStatus2Wrapper");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

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

    // assignment and milestone repeater
    assignmentMilestoneRepeater = new RepeaterWrapper("activity in " + this.controllerVm + ".milestonesActivities");
    assigmentLocator = by.css("a.delete-activity");

    // add assignment button
    addAssignmentButton = $("#addAssignment");

    // add milestone button
    addMilestoneButton = $("#addMilestone");
}

export = ItPojectEditPo;
