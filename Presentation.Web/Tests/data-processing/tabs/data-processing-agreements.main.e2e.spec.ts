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

        DataProcessingAgreementHelper.createDataProcessingAgreement(name)
            .then(() => DataProcessingAgreementHelper.createDataProcessingAgreement(name2))
            .then(() => DataProcessingAgreementHelper.goToSpecificDataProcessingAgreement(name))
            .then(() => expect(getValueAttribute(pageObject.getDpaMainNameInput())).toEqual(name))
            .then(() => pageObject.getDpaMainNameInput().click())
            .then(() => pageObject.getDpaMainNameInput().clear())
            .then(() => pageObject.getDpaMainNameInput().sendKeys(renameValue))
            .then(() => pageObject.getDpaMainNameInput().sendKeys(protractor.Key.TAB))
            .then(() => expect(pageObject.getDpaMainNameHeader().getText()).toEqual(renameValue))
            .then(() => pageObjectOverview.getPage())
            .then(() => expect(pageObjectOverview.findSpecificDpaInNameColumn(renameValue).isPresent()).toBeTruthy())
            .then(() => DataProcessingAgreementHelper.goToSpecificDataProcessingAgreement(name2))
            .then(() => expect(pageObject.getDpaMainNameHeader().getText()).toEqual(name2))
            .then(() => pageObject.getDpaMainNameInput().click())
            .then(() => pageObject.getDpaMainNameInput().clear())
            .then(() => pageObject.getDpaMainNameInput().sendKeys(renameValue))
            .then(() => pageObject.getDpaMainNameInput().sendKeys(protractor.Key.TAB))
            .then(() => expect(pageObject.getDpaMainNameHeader().getText()).toEqual(name2));
    });

    it("It is possible to delete a data processing agreement", () => {

        const name = createName(1);

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
        return pageObject.getDpaDeleteButton();
    }

});