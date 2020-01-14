import Login = require("../Helpers/LoginHelper");
import Misc = require("../PageObjects/Global-admin/GlobalMisc.po");
import Constants = require("../Utility/Constants");
import CSSLocatorHelper = require("../Object-wrappers/CSSLocatorHelper");
import WaitTimers = require("../Utility/WaitTimers");

describe("Global Administrator is able to see changes to KLE and update to the newest KLE version", () => {

    var loginHelper = new Login();
    var consts = new Constants();
    var pageObject = new Misc();
    var cssHelper = new CSSLocatorHelper();
    var waitUpTo = new WaitTimers();

    beforeAll(() => {
        loginHelper.loginAsGlobalAdmin();
    });

    beforeEach(() => {
        pageObject.getPage();
        browser.waitForAngular();
    });

    it("Is able to see if there is an update for KLE", () => {
        expect(element(by.id(consts.kleStatusLabel)).isDisplayed());
    });

    it("Is able to see the button for KLE changes", () => {
        expect(element(by.id(consts.kleChangesButton)).isDisplayed());
    });

    it("Is able to see the button for importing KLE changes", () => {
        expect(element(by.id(consts.kleUpdateButton)).isDisplayed());
    });
     
    it("Is able to check,download and execute a KLE update", () => {
        waitForNewUpdateAvailable().then(() => {
            console.log("Clicking the view changes button");
            return element(cssHelper.byDataElementType(consts.kleChangesButton)).click();
        }).then(() => {
            console.log("Waiting for download link to appear");
            return browser.wait(pageObject.waitForKleDownloadLink(), waitUpTo.twentySeconds);
        }).then(() => {
            console.log("Clicking the update KLE button");
            return element(cssHelper.byDataElementType(consts.kleUpdateButton)).click();
        }).then(() => {
            console.log("Confirming update");
            return browser.switchTo().alert().accept();
        }).then(() => {
            console.log("Checking KLE have been updated");
            //expect(element(cssHelper.byDataElementType(consts.kleStatusLabel)).getText())
            //    .toStartWith("KITOS baserer sig på den seneste KLE version, udgivet");
            expect(element(cssHelper.byDataElementType(consts.kleStatusLabel)).getText())
                .toStartWith("Der er en ny version af KLE, udgivet");
        });
    });

    function waitForNewUpdateAvailable() {
        console.log("Waiting for new update status to return");
        return browser.wait(pageObject.waitForKleChangesButtonToBeActive(), waitUpTo.twentySeconds);
    }

});

