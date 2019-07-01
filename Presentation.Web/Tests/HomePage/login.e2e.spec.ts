import Helper = require("../helper");

describe("Can login succesfully", () => {

    var loginHelper = new Helper.Login();

    beforeAll(() => {
        loginHelper.login("support@kitos.dk", "testpwrd");
        browser.waitForAngular();
    });

    it("Is succesfully logged in", () => {
        //Arrange

        //Act 

        //Assert
        expect(element(by.binding("user.fullName")).getText()).toEqual("Global admin");
    });

});