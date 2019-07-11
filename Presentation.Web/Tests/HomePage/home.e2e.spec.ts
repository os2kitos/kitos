import homePage = require("../PageObjects/HomePage/HomePage.po");

describe("home view", () => {

    var pageObject = new homePage(); 

    beforeEach(() => {
        pageObject.getPage();
    });

    it("should mark invalid email in field", () => {

        pageObject.emailField.sendKeys("invalid-email");

        pageObject.pwdField.sendKeys("");

        expect(pageObject.emailField.getAttribute("class")).toContain(" ng-invalid ");
    });

    it("should mark valid email in field", () => {

        pageObject.emailField.sendKeys("valid-email@valid.dk");

        pageObject.pwdField.sendKeys("");

        expect(pageObject.emailField.getAttribute("class")).toContain(" ng-valid ");
    });
});

