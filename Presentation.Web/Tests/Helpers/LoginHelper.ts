export class Login {

    

    public logout() {
        var logoutBtn = element(by.linkText("Log ud"));
        logoutBtn.click();
    }

    public loginAsGlobalAdmin() {
        this.login(this.parseStringAsArrayAndGetIndex(browser.params.login.email, 0), this.parseStringAsArrayAndGetIndex(browser.params.login.pwd, 0));
    }

    public loginAsLocalAdmin() {
        this.login(this.parseStringAsArrayAndGetIndex(browser.params.login.email, 1), this.parseStringAsArrayAndGetIndex(browser.params.login.pwd, 1));
    }

    public loginAsRegularUser() {
        this.login(this.parseStringAsArrayAndGetIndex(browser.params.login.email, 2), this.parseStringAsArrayAndGetIndex(browser.params.login.pwd, 2));
    }

    private parseStringAsArrayAndGetIndex(input: string, index:number) {
        return input.substring(1, input.length - 1).split(", ")[index];
    }


    private login(usrName: string, pwd: string) {
        browser.get(browser.baseUrl);
        var emailField = element(by.model("email"));
        var passwordField = element(by.model("password"));
        var loginBtn = element(by.buttonText("Log ind"));

        emailField.sendKeys(usrName);
        passwordField.sendKeys(pwd);
        loginBtn.click();
    }
}