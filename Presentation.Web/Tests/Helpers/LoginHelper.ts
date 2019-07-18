import HomePage = require("../PageObjects/HomePage/HomePage.po");
import LoginPage = require("../PageObjects/HomePage/LoginPage.po")
import WaitTimers = require("../Utility/WaitTimers");

var waitUpTo = new WaitTimers();

class Login {
    
    public logout() {
        var navigationBarHelper = new LoginPage().navigationBarHelper;
        navigationBarHelper.logout();
    }

    public loginAsGlobalAdmin() {
        this.login(0);
    }

    public loginAsLocalAdmin() {
        this.login(1);
    }

    public loginAsRegularUser() {
        this.login(2);
    }

    private login(credentialsIndex: number) {
        var homePage = new HomePage();
        homePage.getPage();
        browser.wait(homePage.isLoginAvailable(), waitUpTo.twentySeconds);
        homePage.emailField.sendKeys(this.parseStringAsArrayAndGetIndex(browser.params.login.email, credentialsIndex));
        homePage.pwdField.sendKeys(this.parseStringAsArrayAndGetIndex(browser.params.login.pwd, credentialsIndex));
        homePage.loginButton.click();
    }

    private parseStringAsArrayAndGetIndex(input: string, index: number) {
        return input.substring(1, input.length - 1).split(", ")[index];
    }
}

export = Login;