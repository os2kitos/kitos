import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

class PageObject implements IPageObject {
    controllerVm = "projectPhasesVm";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/phases");
    }

    // date for status update
    editPhaseNamesElement = $("#edit-phase-names");

    // phase name repeater
    nameRepeater = new RepeaterWrapper(`phaseName in ${this.controllerVm}.project.phases`);
    nameLocator = by.css("input:not(.ng-hide)");

    // phase select buttons repeater
    buttonRepeater = new RepeaterWrapper(`phaseButton in ${this.controllerVm}.project.phases`);
    buttonLocator = by.css("button");

    // phase cross date repeater
    crossDateRepeater = new RepeaterWrapper(`phaseCrossDate in ${this.controllerVm}.project.phases`);
    crossDateLocator = by.css("input");
}

export = PageObject;
