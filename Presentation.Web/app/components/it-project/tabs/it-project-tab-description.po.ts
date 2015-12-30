import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");

class ItProjectEditTabDescription implements IPageObject {
    controllerVm: string = "projectDescriptionVm";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/description");
    }

    // note for status
    descriptionElement = element(by.model(this.controllerVm + ".project.description"));
    get descriptionInput(): string {
        var value: string;
        this.descriptionElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set descriptionInput(value: string) {
        this.descriptionElement.sendKeys(value);
    }
}

export = ItProjectEditTabDescription;
