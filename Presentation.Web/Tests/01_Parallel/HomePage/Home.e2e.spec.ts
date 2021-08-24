import HomePage = require("../../PageObjects/HomePage/HomePage.po");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");

describe("home view", () => {
    var testFixture = new TestFixtureWrapper();
    var pageObject = new HomePage();

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