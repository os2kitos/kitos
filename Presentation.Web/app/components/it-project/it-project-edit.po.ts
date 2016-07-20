import IPageObject = require("../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../Tests/object-wrappers/Select2Wrapper");

class ItPojectEditPo implements IPageObject {
    controllerVm = "projectEditVm";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1");
    }

    // delete project
    deleteProjectElement = element(by.className("btn-danger"));

    // name
    nameElement = element(by.model(this.controllerVm + ".project.name"));
    nameInput(value: string) {
        this.nameElement.sendKeys(value);
    }

    // id
    idElement = element(by.model(this.controllerVm + ".project.itProjectId"));
    idInput(value: string) {
        this.idElement.sendKeys(value);
    }

    // type
    typeSelect = new Select2Wrapper("#s2id_project-type");

    // cmdb
    cmdbElement = element(by.model(this.controllerVm + ".project.cmdb"));
    cmdbInput(value: string) {
        this.cmdbElement.sendKeys(value);
    }

    // access
    accessSelect = new Select2Wrapper("#s2id_project-access");

    // esdh
    esdhElement = element(by.model(this.controllerVm + ".project.esdh"));
    esdhInput(value: string) {
        this.esdhElement.sendKeys(value);
    }

    // folder
    folderElement = element(by.model(this.controllerVm + ".project.folder"));
    folderInput(value: string) {
        this.folderElement.sendKeys(value);
    }

    // archive
    archiveCheckbox = element(by.model(this.controllerVm + ".project.isArchived"));

    // transversal
    transversalCheckbox = element(by.model(this.controllerVm + ".project.isTransversal"));

    // strategy
    strategyCheckbox = element(by.model(this.controllerVm + ".project.isStrategy"));

    // background
    backgroundElement = element(by.model(this.controllerVm + ".project.background"));
    backgroundInput(value: string) {
        this.backgroundElement.sendKeys(value);
    }

    // note
    noteElement = element(by.model(this.controllerVm + ".project.note"));
    noteInput(value: string) {
        this.noteElement.sendKeys(value);
    }

    // project parent
    projectParentSelect = new Select2Wrapper("#s2id_project-parent");
}

export = ItPojectEditPo;
