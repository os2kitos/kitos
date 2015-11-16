module Kitos.Tests.e2e.Project.Edit {

    class ProjectEditPo {
        nameInput;

        constructor() {
            this.nameInput = element(by.model('projectEditVm.project.name'));
        }

        get(): void {
            browser.get('https://localhost:44300/#/project/edit/1');
        }

        get name(): string {
            return this.nameInput.getText();
        }

        set name(value: string) {
            this.nameInput.sendKeys(value);
        }
    }

    describe('project edit view', () => {
        var editPage = new ProjectEditPo();

        beforeEach(() => {
            editPage.get();
        });

        it('should save when name looses focus', () => {
            // arrange
            var emailField = editPage.nameInput;
            angular.module('app').requires.push('httpBackendMock');

            // act
            editPage.name = 'some name';

            // assert
            expect(emailField.getAttribute('class')).toMatch('ng-invalid');
        });
    });
}
