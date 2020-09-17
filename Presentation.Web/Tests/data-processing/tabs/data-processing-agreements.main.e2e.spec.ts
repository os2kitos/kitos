"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingAgreementOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../../Utility/WaitTimers");
import DataProcessingAgreementEditMainPageObject = require("../../PageObjects/Data-Processing/Tabs/data-processing-agreement.edit.main.po");
import DataProcessingAgreementHelper = require("../../Helpers/DataProcessingAgreementHelper")

describe("Data processing agreement main detail tests", () => {

    const loginHelper = new Login();
    const pageObjectOverview = new DataProcessingAgreementOverviewPageObject();
    const pageObject = new DataProcessingAgreementEditMainPageObject();
    const testFixture = new TestFixtureWrapper();
    const waitUpTo = new WaitTimers();
    const ec = protractor.ExpectedConditions;
    const dpaHelper = DataProcessingAgreementHelper;

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


    it("Creating and renaming data processing agreement",
        () => {
            var name = createName(10);
            var renameValue = createName(30);

            dpaHelper.createDataProcessingAgreement(name)
                .then(() => pageObjectOverview.findSpecificDpaInNameColumn(name))
                .then(() => dpaHelper.goToSpecificDataProcessingAgreement(name))
                .then(() => renameNameAndVerify(renameValue));
        });


    it("It is possible to delete a data processing agreement",
        () => {
            const nameToDelete = createName(1);
            console.log("Creating agreement and deleting it");

            dpaHelper.createDataProcessingAgreement(nameToDelete)
                .then(() => dpaHelper.goToSpecificDataProcessingAgreement(nameToDelete))
                .then(() => getDeleteButtonAndDelete())
                .then(() => expect(pageObjectOverview.findSpecificDpaInNameColumn(nameToDelete).isPresent())
                    .toBeFalsy());
        });

    function renameNameAndVerify(name: string) {
        console.log(`Renaming agreement to ${name}`);
        pageObject.getDpaMainNameInput().click()
            .then(() => pageObject.getDpaMainNameInput().clear())
            .then(() => pageObject.getDpaMainNameInput().sendKeys(name))
            .then(() => pageObject.getDpaMainNameInput().sendKeys(protractor.Key.TAB))
            .then(() => {
                console.log(`Expecting agreement to be called ${name}`);
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