module Kitos.Tests.e2e.Home {
    class KitosHomePage {
        emailInput;

        constructor() {
            this.emailInput = element(by.model('email'));
        }

        get(): void {
            browser.get('https://localhost:44300');
        }

        get email(): string {
            return this.emailInput.getText();
        }

        set email(value: string) {
            this.emailInput.sendKeys(value);
        }
    }

    describe('home view', () => {
        var homePage;

        beforeEach(() => {
            homePage = new KitosHomePage();
            homePage.get();
        });

        it('should mark invalid email in field', () => {
            // arrange
            var emailField = homePage.emailInput;

            // act
            homePage.email = 'some invalid email';

            // assert
            expect(emailField.getAttribute('class')).toMatch('ng-invalid');
        });

        it('should mark valid email in field', () => {
            // arrange
            var emailField = homePage.emailInput;
            // act
            homePage.email = 'some@email.test';

            // assert
            expect(emailField.getAttribute('class')).toMatch('ng-valid');
        });

        it('shouldfail always for testing', () => {
            // arrange
            var emailField = homePage.emailInput;
            // act
            homePage.email = 'some@email.test';

            // assert
            expect(emailField.getAttribute('class')).toMatch('nonexistingclass');
        });
    });
}
