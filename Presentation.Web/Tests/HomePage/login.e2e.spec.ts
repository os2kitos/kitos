import login = require("../Helpers/LoginHelper");

describe("Can login succesfully", () => { 

    var loginHelper = new login.Login();

    beforeAll(() => {
        loginHelper.loginAsGlobalAdmin();
        browser.waitForAngular();
    });

    it("Is succesfully logged in", () => {
        //Arrange

        //Act 

        //Assert
        expect(browser.params.login.email.substring(1, browser.params.login.email.length - 1).split(", ")[0]).toEqual("support@kitos.dk");
    });

    it("Is succesfully logged in", () => {
        //Arrange

        //Act 

        //Assert
        expect(element(by.binding("user.fullName")).getText()).toEqual("Global admin");
    });

});