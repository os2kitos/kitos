import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");

class ItProjectEditTabDescription implements IPageObject {
    controllerVm = "projectDescriptionVm";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/description");
    }

    // note for status
    descriptionElement = element(by.model(this.controllerVm + ".project.description"));
    descriptionInput(value: string) {
        this.descriptionElement.sendKeys(value);
    }
}

export = ItProjectEditTabDescription;
