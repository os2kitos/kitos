import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import SelectStatus2Wrapper = require("../../../../Tests/object-wrappers/SelectStatus2Wrapper");

class ItProjectEditTabStrategy implements IPageObject {
    controllerVm = "projectStatusVm";

    getPage(): webdriver.promise.Promise<void> {
        throw Error("This PO covers two modal pages. Use getAssignmentPage() or getMilestonePage()");
    }

    getAssignmentPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/status-project/modal/assignment/1");
    }

    getMilestonePage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/status-project/modal/milestone/1");
    }

    // name input
    nameElement = element(by.css("#name"));
    nameInput = (value: string) => {
        this.nameElement.sendKeys(value);
    }

    // humanReadableId input
    humanReadableIdElement = element(by.css("#humanReadableId"));
    humanReadableIdInput = (value: string) => {
        this.humanReadableIdElement.sendKeys(value);
    }

    // phase selector
    phaseSelector = element(by.css("#phase"));

    // startDate input
    startDateElement = element(by.css("#startDate"));
    startDateInput = (value: string) => {
        this.startDateElement.sendKeys(value);
    }

    // endDate input
    endDateElement = element(by.css("#endDate"));
    endDateInput = (value: string) => {
        this.endDateElement.sendKeys(value);
    }

    // date input
    dateElement = element(by.css("#date"));
    dateInput = (value: string) => {
        this.dateElement.sendKeys(value);
    }


    // timeEstimate input
    timeEstimateElement = element(by.css("#timeEstimate"));
    timeEstimateInput = (value: string) => {
        this.timeEstimateElement.sendKeys(value);
    }

    // status selector
    statusSelector = element(by.css("#status"));

    // milestone status selector
    milestoneStatusSelector = new SelectStatus2Wrapper("#milestone-status");

    // description input
    descriptionElement = element(by.css("#description"));
    descriptionInput = (value: string) => {
        this.descriptionElement.sendKeys(value);
    }

    // note input
    noteElement = element(by.css("#assignment-note"));
    noteInput = (value: string) => {
        this.noteElement.sendKeys(value);
    }

    // associatedUser input
    associatedUserElement = element(by.css("#associatedUserId"));

    // save button
    saveButton = element(by.css("#save"));

    // cancel button
    cancelButton = element(by.css("#cancel"));

    // close button
    closeButton = element(by.css("#close"));
}

export = ItProjectEditTabStrategy;
