"use strict";
import Login = require("../Helpers/LoginHelper");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import DataProcessingAgreementOverviewPageObject =
require("../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../Utility/WaitTimers");
import LocalDataProcessing = require("../PageObjects/Local-admin/LocalDataProcessing.po");

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

    it("It is possible to create new data processing agreements", () => {
        const name = createName(1);
        pageObject
            //Verify correct creation of dpa and following update of kendo
            .getPage()
            .then(() => waitForKendo())
            .then(() => openNewDpaDialog())
            .then(() => enterDpaName(name))
            .then(() => {
                validateSaveDpaClickable(true);
                console.log(`clicking 'save'`);
                return pageObject.getNewDpaSubmitButton().click();
            })
            .then(() => waitForKendo())
            .then(() => {
                console.log(`expecting to find new dpa with name ${name}`);
                expect(pageObject.findSpecificDpaInNameColumn(name).isPresent()).toBeTruthy();
            })
            //Verify that dialog acts correctly on attempt to create duplicate dpa name
            .then(() => openNewDpaDialog())
            .then(() => {
                console.log(`Validating UI error when entering the same name again.`);
                return enterDpaName(name);
            })
            .then(() => {
                console.log("Waiting for UI error to appear");
                browser.wait(ec.not(ec.elementToBeClickable(pageObject.getNewDpaSubmitButton())), waitUpTo.twentySeconds);
                validateSaveDpaClickable(false);
            });
    });

    function openNewDpaDialog() {
        console.log("clicking createDpaButton");
        return pageObject.getCreateDpaButton().click()
            .then(() => {
            console.log("waiting for dialog to be visible");
            return browser.wait(pageObject.isCreateDpaAvailable(), waitUpTo.twentySeconds);
        });
    }

    function validateSaveDpaClickable(isClickable: boolean) {
        console.log(`Expecting 'save' have clickable state equal ${isClickable}`);
        const expectation = expect(pageObject.getNewDpaSubmitButton().isEnabled());
        return isClickable ? expectation.toBeTruthy() : expectation.toBeFalsy();
    }

    function enterDpaName(name : string) {
        console.log(`entering name: '${name}'`);
        return pageObject.getNewDpaNameInput().sendKeys(name);
    }

    function waitForKendo() {
        console.log("waiting for kendo grid to load");
        return pageObject.waitForKendoGrid();
    }
});