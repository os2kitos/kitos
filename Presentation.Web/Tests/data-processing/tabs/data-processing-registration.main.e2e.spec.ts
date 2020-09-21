"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingRegistrationOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-registration.overview.po");
import WaitTimers = require("../../Utility/WaitTimers");
import DataProcessingRegistrationEditMainPageObject = require("../../PageObjects/Data-Processing/Tabs/data-processing-registration.edit.main.po");
import DataProcessingRegistrationHelper = require("../../Helpers/DataProcessingRegistrationHelper");

describe("Data processing agreement main detail tests", () => {

    const loginHelper = new Login();
    const pageObjectOverview = new DataProcessingRegistrationOverviewPageObject();
    const pageObject = new DataProcessingRegistrationEditMainPageObject();
    const testFixture = new TestFixtureWrapper();
    const waitUpTo = new WaitTimers();
    const ec = protractor.ExpectedConditions;
    const dpaHelper = DataProcessingRegistrationHelper;

    const createName = (index: number) => {
        return `Dpa${new Date().getTime()}_${index}`;
    }

    beforeAll(() => {
        loginHelper.loginAsLocalAdmin();
        testFixture.enableLongRunningTest();
        dpaHelper.checkAndEnableDpaModule();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });


    it("Creating and renaming data processing registration",
        () => {
            var name = createName(10);
            var renameValue = createName(30);

            dpaHelper.createDataProcessingRegistration(name)
                .then(() => pageObjectOverview.findSpecificDpaInNameColumn(name))
                .then(() => dpaHelper.goToSpecificDataProcessingRegistration(name))
                .then(() => renameNameAndVerify(renameValue));
        });


    it("It is possible to delete a data processing registration",
        () => {
            const nameToDelete = createName(1);
            console.log("Creating agreement and deleting it");

            dpaHelper.createDataProcessingRegistration(nameToDelete)
                .then(() => dpaHelper.goToSpecificDataProcessingRegistration(nameToDelete))
                .then(() => getDeleteButtonAndDelete())
                .then(() => dpaHelper.loadOverview())
                .then(() => expect(pageObjectOverview.findSpecificDpaInNameColumn(nameToDelete).isPresent())
                    .toBeFalsy());
        });

    function renameNameAndVerify(name: string) {
        console.log(`Renaming registration to ${name}`);
        pageObject.getDpaMainNameInput().click()
            .then(() => pageObject.getDpaMainNameInput().clear())
            .then(() => pageObject.getDpaMainNameInput().sendKeys(name))
            .then(() => pageObject.getDpaMainNameInput().sendKeys(protractor.Key.TAB))
            .then(() => {
                console.log(`Expecting registration to be called ${name}`);
                browser.wait(ec.textToBePresentInElement(pageObject.getDpaMainNameHeader(), name),
                    waitUpTo.twentySeconds,
                    `Could not verify that ${name} was changed`);
            });
    }

    function getDeleteButtonAndDelete() {
        console.log("Retrieving deletebutton");
        return pageObject.getDpaDeleteButton().click()
            .then(() => browser.switchTo().alert().accept());
    }

});