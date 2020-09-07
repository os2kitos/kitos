import Login = require("../Helpers/LoginHelper");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import DataProcessingAgreementOverviewPageObject =
require("../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../Utility/WaitTimers");

describe("Data processing agreement tests", () => {

    const loginHelper = new Login();
    const pageObject = new DataProcessingAgreementOverviewPageObject();
    const testFixture = new TestFixtureWrapper();
    const waitUpTo = new WaitTimers();
    const ec = protractor.ExpectedConditions;

    const createName = (index: number) => {
        return `Dpa${new Date().getTime()}_${index}`;
    }

    beforeAll(() => {
        loginHelper.loginAsLocalAdmin();
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("It is possible to create new data processing agreements", () => {
        const name = createName(1);
        pageObject
            //Verify correct creation of dpa and following update of kendo
            .getPage()
            .then(() => waitForKendo())
            .then(() => {
                console.log("clicking createDpaButton");
                return pageObject.getCreateDpaButton().click();
            })
            .then(() => {
                console.log("waiting for dialog to be visible");
                return browser.wait(pageObject.isCreateDpaAvailable(), waitUpTo.twentySeconds);
            })
            .then(() => {
                console.log(`entering name: '${name}'`);
                return pageObject.getNewDpaNameInput().sendKeys(name);
            })
            .then(() => {
                console.log(`Expecting 'save' to be clickable`);
                expect(pageObject.getNewDpaSubmitButton().isEnabled()).toBeTruthy();
                console.log(`clicking 'save'`);
                return pageObject.getNewDpaSubmitButton().click();
            })
            .then(() => waitForKendo())
            .then(() => {
                console.log(`expecting to find new dpa with name ${name}`);
                expect(pageObject.findSpecificDpaInNameColumn(name).isPresent()).toBeTruthy();
            })
            //Verify that dialog acts correctly on attempt to create duplicate dpa name
            .then(() => {
                console.log("clicking createDpaButton");
                return pageObject.getCreateDpaButton().click();
            })
            .then(() => {
                console.log("waiting for dialog to be visible");
                return browser.wait(pageObject.isCreateDpaAvailable(), waitUpTo.twentySeconds);
            })
            .then(() => {
                console.log(`entering SAME name again: '${name}'`);
                return pageObject.getNewDpaNameInput().sendKeys(name);
            })
            .then(() => {
                console.log(`Expecting 'save' to be disabled`);
                browser.wait(ec.not(ec.elementToBeClickable(pageObject.getNewDpaSubmitButton())), waitUpTo.twentySeconds);
                expect(pageObject.getNewDpaSubmitButton().isEnabled()).toBeFalsy();
            });
    });

    function waitForKendo() {
        console.log("waiting for kendo grid to load");
        return pageObject.waitForKendoGrid();
    }
});