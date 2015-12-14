import IPageObject = require("../../IPageObject.po");
import Select2Wrapper = require("../../Select2Wrapper");

class ItPojectEditPo implements IPageObject {
    controllerVm: string = "projectEditVm";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/status-project");
    }

    // name
    nameElement = element(by.model(this.controllerVm + ".project.name"));
    get nameInput(): string {
        var value: string;
        this.nameElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set nameInput(value: string) {
        this.nameElement.sendKeys(value);
    }

    // id
    idElement = element(by.model(this.controllerVm + ".project.itProjectId"));
    get idInput(): string {
        var value: string;
        this.idElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set idInput(value: string) {
        this.idElement.sendKeys(value);
    }

    // type
    typeSelect = new Select2Wrapper("#s2id_project-type");

    // cmdb
    cmdbElement = element(by.model(this.controllerVm + ".project.cmdb"));
    get cmdbInput(): string {
        var value: string;
        this.cmdbElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set cmdbInput(value: string) {
        this.cmdbElement.sendKeys(value);
    }

    // esdh
    esdhElement = element(by.model(this.controllerVm + ".project.esdh"));
    get esdhInput(): string {
        var value: string;
        this.esdhElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set esdhInput(value: string) {
        this.esdhElement.sendKeys(value);
    }

    // folder
    folderElement = element(by.model(this.controllerVm + ".project.folder"));
    get folderInput(): string {
        var value: string;
        this.folderElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set folderInput(value: string) {
        this.folderElement.sendKeys(value);
    }

    // background
    backgroundElement = element(by.model(this.controllerVm + ".project.background"));
    get backgroundInput(): string {
        var value: string;
        this.backgroundElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set backgroundInput(value: string) {
        this.backgroundElement.sendKeys(value);
    }

    // note
    noteElement = element(by.model(this.controllerVm + ".project.note"));
    get noteInput(): string {
        var value: string;
        this.noteElement.getAttribute("value").then(v => value = v);
        return value;
    }

    set noteInput(value: string) {
        this.noteElement.sendKeys(value);
    }
}

export = ItPojectEditPo;
