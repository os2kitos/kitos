import IPageObject = require('../../IPageObject.po');

class ItPojectEditPo implements IPageObject {

    getPage(): void {
        browser.get('https://localhost:44300/#/project/edit/1/status-project');
    }

    nameElement = element(by.id('project-name'));
    get nameInput(): string {
        var value: string;
        this.nameElement.getAttribute('value').then(v => value = v);
        return value;
    }

    set nameInput(value: string) {
        this.nameElement.sendKeys(value);
    }

    idElement = element(by.id('project-projectId'));
    get idInput(): string {
        var value: string;
        this.idElement.getAttribute('value').then(v => value = v);
        return value;
    }

    set idInput(value: string) {
        this.idElement.sendKeys(value);
    }
}

export = ItPojectEditPo;
