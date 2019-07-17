import homePage = require("../PageObjects/HomePage/HomePage.po");
import testFixtureWrapper = require("../Utility/TestFixtureWrapper");

var testFixture = new testFixtureWrapper();
var pageObject = new homePage();

describe("home view", () => {

    beforeEach(() => {
        pageObject.getPage();
    });

    afterEach(() => {
        testFixture.cleanupState();
    });

    it("should mark invalid email in field", () => {
        pageObject.emailField.sendKeys("invalid-email.com");

        expect(pageObject.emailField.getAttribute("class")).toContain(" ng-invalid ");
    });

});