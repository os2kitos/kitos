import HomePage = require("../PageObjects/HomePage/HomePage.po");
import LoginPage = require("../PageObjects/HomePage/LoginPage.po")
import WaitTimers = require("../Utility/WaitTimers");

class Login {
    public logout() {
        var navigationBarHelper = new LoginPage().navigationBarHelper;
        navigationBarHelper.dropDownExpand();
        return navigationBarHelper.logout();
    }

    public loginAsGlobalAdmin() {
        const credentials = this.getCredentialsMap().globalAdmin;
        return this.login(credentials);
    }

    public loginAsLocalAdmin() {
        const credentials = this.getCredentialsMap().localAdmin;
        return this.login(credentials);
    }

    public loginAsRegularUser() {
        return this.login(this.getCredentialsMap().regularUser);
    }

    public loginAsApiUser() {
        return this.login(this.getCredentialsMap().apiUsers.regularUser);
    }

    public getApiUserCredentials() {
        return this.getCredentialsMap().apiUsers.regularUser;
    }

    public getLocalAdminCredentials() {
        return this.getCredentialsMap().localAdmin;
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

    private login(credentials: any, organization?: string) {
        var homePage = new HomePage();
        var navigationBar = new LoginPage().navigationBar;
        var waitUpTo = new WaitTimers();
        var ec = protractor.ExpectedConditions;

        return homePage.getPage()
            .then(() => browser.wait(homePage.isLoginAvailable(), waitUpTo.twentySeconds))
            .then(() => homePage.emailField.sendKeys(credentials.username))
            .then(() => homePage.pwdField.sendKeys(credentials.password))
            .then(() => homePage.loginButton.click())
            .then(() => browser.waitForAngular())
            .then(() => {
                if (credentials.username === this.getCredentialsMap().globalAdmin.username) {
                    console.log("User is global admin - must select organization before proceeding");
                    return browser.wait(ec.visibilityOf(homePage.selectWorkingOrganizationDialog),waitUpTo.twentySeconds)
                        .then(() => homePage.selectDefaultOrganizationAsWorkingOrg())
                        .then(() => homePage.selectWorkingOrganizationButton.click());
                }
                else {
                    console.log(`Logging in as user other than global or local admin`);
                    return true;
                }
            })
            .then(() => browser.wait(ec.visibilityOf(navigationBar.dropDownMenu.dropDownElement), waitUpTo.twentySeconds));
    }

    private parseStringAsArrayAndGetIndex(input: string, index: number) {
        return String(input).substring(1, input.length - 1).split(", ")[index];
    }
}

export = Login;