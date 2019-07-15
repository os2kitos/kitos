import homePage = require("../PageObjects/HomePage/HomePage.po");

describe("home view", () => {

    var pageObject = new homePage();

    beforeEach(() => {
        pageObject.getPage();
    });

    afterEach(() => {
        browser.driver.manage().deleteAllCookies();
    });

    it("should mark invalid email in field", () => {
        pageObject.emailField.sendKeys("invalid-email.com");

        expect(pageObject.emailField.getAttribute("class")).toContain(" ng-invalid ");
    });

});