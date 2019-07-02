module Kitos.Tests.e2e.Home {
    class KitosHomePage {
        public  emailInput;
        public  passwordInput;
        public loginButton;

        constructor() {
            this.emailInput = element(by.model("email"));
            this.passwordInput = element(by.model("password"));
            this.loginButton = element(by.buttonText('Log ind'));
        }

        get(): void {
            browser.get(browser.baseUrl);
        }

        get email(): string {
            return this.emailInput.getText();
        }

        set email(value: string) {
            this.emailInput.sendKeys(value);
        }

        get password(): string {
            return this.passwordInput.getText();
        }

        set password(value: string) {
            this.passwordInput.sendKeys(value);
        }
    }

    describe("home view", () => {
        var homePage;

        beforeEach(() => {
            homePage = new KitosHomePage();
            homePage.get();
        });

        it("should mark invalid email in field", () => {
            // arrange
            var emailField = homePage.emailInput;

            // act
            homePage.email = "some invalid email";

            // assert
            expect(emailField.getAttribute("class")).toMatch("ng-invalid");
        });

        it("should mark valid email in field", () => {
            // arrange
            var emailField = homePage.emailInput;
            // act
            homePage.email = "some@email.test";

            // assert
            expect(emailField.getAttribute("class")).toMatch("ng-valid");
        });
    });
}
