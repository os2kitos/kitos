"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingAgreementOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../../Utility/WaitTimers");
import LocalDataProcessing = require("../../PageObjects/Local-admin/LocalDataProcessing.po");
import DataProcessingAgreementHelper = require("../../Helpers/DataProcessingAgreementHelper");
import DataProcessingAgreementEditMainPageObject = require("../../PageObjects/Data-Processing/Tabs/data-processing-agreement.edit.main");
import CssHelper = require("../../Object-wrappers/CSSLocatorHelper");

describe("Data processing agreement main detail tests", () => {

    const loginHelper = new Login();
    const pageObjectOverview = new DataProcessingAgreementOverviewPageObject();
    const pageObject = new DataProcessingAgreementEditMainPageObject();
    const testFixture = new TestFixtureWrapper();
    const waitUpTo = new WaitTimers();
    const ec = protractor.ExpectedConditions;
    const cssHelper = new CssHelper();

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

    it("It is possible to rename a data processing agreement", () => {

        const name = createName(1);
        const name2 = createName(2);
        const renameValue = createName(3);
        console.log("Creating Data Processing Agreements");
        DataProcessingAgreementHelper.createDataProcessingAgreement(name)
            .then(() => DataProcessingAgreementHelper.createDataProcessingAgreement(name2))
            .then(() => DataProcessingAgreementHelper.goToSpecificDataProcessingAgreement(name))
            .then(() => expect(getValueAttribute(pageObject.getDpaMainNameInput())).toEqual(name))
            .then(() => {
                console.log("Renaming agreement and checking if value is saved");
                pageObject.getDpaMainNameInput().click();
            })
            .then(() => pageObject.getDpaMainNameInput().clear())
            .then(() => pageObject.getDpaMainNameInput().sendKeys(renameValue))
            .then(() => pageObject.getDpaMainNameInput().sendKeys(protractor.Key.TAB))
            .then(() => waitForHeaderTitleToChange(pageObject.getDpaMainNameHeader(), renameValue))
            .then(() => pageObjectOverview.getPage())
            .then(() => expect(pageObjectOverview.findSpecificDpaInNameColumn(renameValue).isPresent()).toBeTruthy())
            .then(() => DataProcessingAgreementHelper.goToSpecificDataProcessingAgreement(name2))
            .then(() => expect(pageObject.getDpaMainNameHeader().getText()).toEqual(name2))
            .then(() => {
                console.log("Renaming agreement to already existing agreement");
                pageObject.getDpaMainNameInput().click();
            })
            .then(() => pageObject.getDpaMainNameInput().clear())
            .then(() => pageObject.getDpaMainNameInput().sendKeys(renameValue))
            .then(() => pageObject.getDpaMainNameInput().sendKeys(protractor.Key.TAB))
            .then(() => waitForHeaderTitleToChange(pageObject.getDpaMainNameHeader(), name2));
    });

    it("It is possible to delete a data processing agreement", () => {

        const name = createName(1);
        console.log("Creating agreement and deleting it");
        DataProcessingAgreementHelper.createDataProcessingAgreement(name)
            .then(() => pageObjectOverview.getPage())
            .then(() => expect(pageObjectOverview.findSpecificDpaInNameColumn(name).isPresent()).toBeTruthy())
            .then(() => DataProcessingAgreementHelper.goToSpecificDataProcessingAgreement(name))
            .then(() => getDeleteButton().click())
            .then(() => browser.switchTo().alert().accept())
            .then(() => pageObjectOverview.getPage())
            .then(() => expect(pageObjectOverview.findSpecificDpaInNameColumn(name).isPresent()).toBeFalsy());
    });


    function getValueAttribute(element: protractor.ElementFinder) {
        return element.getAttribute("value");
    }

    function getDeleteButton() {
        console.log("Retrieving deletebutton");
        return pageObject.getDpaDeleteButton();
    }

    function waitForHeaderTitleToChange(element: protractor.ElementFinder, text: string) {
        console.log("Waiting for header title to change to " + text);
        return browser.wait(ec.textToBePresentInElement(element, text), waitUpTo.twentySeconds);
    }

});