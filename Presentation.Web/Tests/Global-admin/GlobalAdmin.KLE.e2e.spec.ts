import Login = require("../Helpers/LoginHelper");
import Misc = require("../PageObjects/Global-admin/GlobalMisc.po");
import Constants = require("../Utility/Constants");
import CSSLocatorHelper = require("../Object-wrappers/CSSLocatorHelper");
import WaitTimers = require("../Utility/WaitTimers");
import NaviHelp = require("../Utility/NavigationHelper");

describe("Global Administrator is able to see changes to KLE and update to the newest KLE version", () => {

    var loginHelper = new Login();
    var consts = new Constants();
    var pageObject = new Misc();
    var cssHelper = new CSSLocatorHelper();
    var waitUpTo = new WaitTimers();
    var navHelper = new NaviHelp();

    beforeAll(() => {
        loginHelper.loginAsGlobalAdmin();
    });

    beforeEach(() => {
        pageObject.getPage();
        browser.waitForAngular();
    });

    it("Is able to see if there is an update for KLE", () => {
        checkIfButtonIsDisplayed(consts.kleStatusLabel);
    });

    it("Is able to see the button for KLE changes", () => {
        checkIfButtonIsDisplayed(consts.kleChangesButton);
    });

    it("Is able to see the button for importing KLE changes", () => {
        checkIfButtonIsDisplayed(consts.kleUpdateButton);
    });
     
    it("Is able to check,download and execute a KLE update", () => {
        waitForNewUpdateAvailable().then(() => {
            console.log("Clicking the view changes button");
            clickOnButton(consts.kleChangesButton);
        }).then(() => {
            console.log("Waiting for download link to appear");
            return browser.wait(pageObject.waitForKleDownloadLink(), waitUpTo.twentySeconds);
        }).then(() => {
            console.log("Clicking the update KLE button");
            clickOnButton(consts.kleUpdateButton);
        }).then(() => {
            console.log("Confirming update");
            navHelper.acceptAlertBox();
        }).then(() => {
            console.log("Checking KLE have been updated");
            expect(element(cssHelper.byDataElementType(consts.kleStatusLabel)).getText())
                .toStartWith("KITOS baserer sig på den seneste KLE version, udgivet");
            expectButtonEnableToBe(consts.kleChangesButton,false);
            expectButtonEnableToBe(consts.kleUpdateButton, false);
        });
    });

    function waitForNewUpdateAvailable() {
        console.log("Waiting for new update status to return");
        return browser.wait(pageObject.waitForKleChangesButtonToBeActive(), waitUpTo.twentySeconds);
    }

    function checkIfButtonIsDisplayed(id: string) {
        expect(element(by.id(id)).isDisplayed());
    }

    function clickOnButton(eleType: string) {
        return element(cssHelper.byDataElementType(eleType)).click();
    }

    function expectButtonEnableToBe(eleType: string,toBe: boolean) {
        expect(element(cssHelper.byDataElementType(eleType)).isEnabled()).toBe(toBe);
    }


});

