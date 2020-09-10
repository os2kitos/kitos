"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingAgreementOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../../Utility/WaitTimers");
import LocalDataProcessing = require("../../PageObjects/Local-admin/LocalDataProcessing.po");
import DataProcessingAgreementEditMainPageObject = require("../../PageObjects/Data-Processing/Tabs/data-processing-agreement.edit.main");

describe("Data processing agreement main detail tests", () => {

    const loginHelper = new Login();
    const pageObjectOverview = new DataProcessingAgreementOverviewPageObject();
    const pageObject = new DataProcessingAgreementEditMainPageObject();
    const testFixture = new TestFixtureWrapper();
    const waitUpTo = new WaitTimers();
    const ec = protractor.ExpectedConditions;

    const createName = (index: number) => {
        return `Dpa${new Date().getTime()}_${index}`;
    }

    beforeAll(() => {
        loginHelper.loginAsLocalAdmin();
        testFixture.enableLongRunningTest();
        var localDpPo = new LocalDataProcessing();
        localDpPo
            .getPage()
            .then(() => localDpPo.getToggleDataProcessingCheckbox().isSelected())
            .then((selected) => {
                if (!selected) {
                    localDpPo.getToggleDataProcessingCheckbox().click();
                }
            });
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });


    it("Creating and renaming data processing agreement",
        () => {
            var name1 = createName(10);
            var renameValue = createName(30);

            pageObjectOverview.getPage()
                .then(() => pageObjectOverview.waitForKendoGrid())
                .then(() => pageObjectOverview.getCreateDpaButton().click())
                .then(() => enterDpaName(name1))
                .then(() => pageObjectOverview.getNewDpaSubmitButton().click())
                .then(() => pageObjectOverview.getPage())
                .then(() => pageObjectOverview.findSpecificDpaInNameColumn(name1))
                .then(() => goToSpecificDataProcessingAgreement(name1))
                .then(() => {
                    console.log("Renaming agreement and checking if value is saved");
                    pageObject.getDpaMainNameInput().click();
                })
                .then(() => pageObject.getDpaMainNameInput().clear())
                .then(() => pageObject.getDpaMainNameInput().sendKeys(renameValue))
                .then(() => pageObject.getDpaMainNameInput().sendKeys(protractor.Key.TAB))
                .then(() => browser.wait(ec.textToBePresentInElement(pageObject.getDpaMainNameHeader(), renameValue),
                    waitUpTo.twentySeconds,
                    "Could not find text specified"));
        });


    it("It is possible to delete a data processing agreement",
        () => {
            const nameDeleted = createName(1);
            console.log("Creating agreement and deleting it");

            pageObjectOverview.getPage()
                .then(() => pageObjectOverview.waitForKendoGrid())
                .then(() => pageObjectOverview.getCreateDpaButton().click())
                .then(() => enterDpaName(nameDeleted))
                .then(() => pageObjectOverview.getNewDpaSubmitButton().click())
                .then(() => pageObjectOverview.getPage())
                .then(() => goToSpecificDataProcessingAgreement(nameDeleted))
                .then(() => getDeleteButton().click())
                .then(() => browser.switchTo().alert().accept())
                .then(() => expect(pageObjectOverview.findSpecificDpaInNameColumn(nameDeleted).isPresent())
                    .toBeFalsy());
        });

    function enterDpaName(name: string) {
        console.log(`entering name: '${name}'`);
        return pageObjectOverview.getNewDpaNameInput().sendKeys(name);
    }

    function goToSpecificDataProcessingAgreement(name: string) {
        console.log("Finding DataProcessingAgreement: " + name);
        return pageObjectOverview.getPage()
            .then(() => pageObjectOverview.waitForKendoGrid())
            .then(() => findDataProcessingAgreementColumnFor(name).first().click());
    }

    function getDeleteButton() {
        console.log("Retrieving deletebutton");
        return pageObject.getDpaDeleteButton();
    }

    function findDataProcessingAgreementColumnFor(name: string) {
        return pageObjectOverview.kendoToolbarWrapper.getFilteredColumnElement(
            pageObjectOverview.kendoToolbarWrapper.columnObjects().dpaName,
            name);
    }

});