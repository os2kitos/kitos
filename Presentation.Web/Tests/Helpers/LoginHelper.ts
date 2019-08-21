import HomePage = require("../PageObjects/HomePage/HomePage.po");
import LoginPage = require("../PageObjects/HomePage/LoginPage.po")
import WaitTimers = require("../Utility/WaitTimers");

class Login {
    public logout() {
        var navigationBarHelper = new LoginPage().navigationBarHelper;
        navigationBarHelper.logout();
    }

    public loginAsGlobalAdmin() {
        this.login(this.getCredentialsMap().globalAdmin);
    }

    public loginAsLocalAdmin() {
        this.login(this.getCredentialsMap().localAdmin);
    }

    public loginAsRegularUser() {
        this.login(this.getCredentialsMap().regularUser);
    }

    public loginAsApiUser() {
        this.login(this.getCredentialsMap().apiUsers.regularUser);
    }

    public getApiUserCredentials() {
        return this.getCredentialsMap().apiUsers.regularUser;
    }

    private getCredentialsMap() {
        return {
            globalAdmin: this.getCredentials(0),
            localAdmin: this.getCredentials(1),
            regularUser: this.getCredentials(2),
            apiUsers: {
                regularUser: this.getCredentials(3)
            }
        };
    }

    private getCredentials(credentialsIndex: number) {
        return {
            username: this.parseStringAsArrayAndGetIndex(browser.params.login.email, credentialsIndex),
            password: this.parseStringAsArrayAndGetIndex(browser.params.login.pwd, credentialsIndex)
        };
    }

    private login(credentials: any) {
        var homePage = new HomePage();
        var navigationBar = new LoginPage().navigationBar;
        var waitUpTo = new WaitTimers();
        var ec = protractor.ExpectedConditions;

        homePage.getPage();
        browser.wait(homePage.isLoginAvailable(), waitUpTo.twentySeconds);
        homePage.emailField.sendKeys(credentials.username);
        homePage.pwdField.sendKeys(credentials.password);
        homePage.loginButton.click();

        //Await login completed before completing command
        browser.wait(ec.visibilityOf(navigationBar.dropDownMenu.dropDownElement), waitUpTo.twentySeconds);
    }

    private parseStringAsArrayAndGetIndex(input: string, index: number) {
        return input.substring(1, input.length - 1).split(", ")[index];
    }
}

export = Login;