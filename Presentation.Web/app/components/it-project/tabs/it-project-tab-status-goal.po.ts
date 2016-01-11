import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import SelectStatus2Wrapper = require("../../../../Tests/object-wrappers/SelectStatus2Wrapper");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

class ItPojectEditTabStatusGoal implements IPageObject {
    controllerVm = "";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/status-goal");
    }

    // status traffic light
    trafficLightSelect = new SelectStatus2Wrapper("#statusGoal");

    // date for status update
    updateDateElement = element(by.css("#statusDate"));
    updateInput(value: string) {
        this.updateDateElement.sendKeys(value);
    }

    // note for status
    noteElement = element(by.css("#note"));
    noteInput(value: string) {
        this.noteElement.sendKeys(value);
    }

    // goal repeater
    goalRepeater = new RepeaterWrapper(`goal in ${this.controllerVm}goals`);
    editGoalLocator = by.css(".edit-goal");
    deleteGoalLocator = by.css(".delete-goal");

    // add goal button
    addGoalButton = $("#add-goal");
}

export = ItPojectEditTabStatusGoal;
