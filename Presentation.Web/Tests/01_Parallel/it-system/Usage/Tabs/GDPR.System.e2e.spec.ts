import login = require("../../../../Helpers/LoginHelper");
import ItSystemCatalogHelper = require("../../../../Helpers/SystemCatalogHelper");
import ItSystemHelper = require("../../../../Helpers/SystemUsageHelper");
import TestFixtureWrapper = require("../../../../Utility/TestFixtureWrapper");
import ItSystemUsageGDPRPage = require("../../../../PageObjects/It-system/Usage/Tabs/ItSystemUsageGDPR.po");
import LocalSystemNavigation = require("../../../../Helpers/SideNavigation/LocalItSystemNavigation");
import Constants = require("../../../../Utility/Constants");
import CssHelper = require("../../../../Object-wrappers/CSSLocatorHelper");
import Select2Helper = require("../../../../Helpers/Select2Helper");

describe("Global admin is able to", () => {

    var loginHelper = new login();
    var testFixture = new TestFixtureWrapper();
    var itSystem1 = createItSystemName();
    var consts = new Constants();
    var cssHelper = new CssHelper();


    var dropdownYes = "Ja";
    var preRiskAssessmentMiddle = "Mellem risiko";
    var hostedValue = "Eksternt";
    var dateValue = getDate();
    var testUrl = "https://www.strongminds.dk/";
    var numberValue = new Date().getDay().toString();

    var purposeText = `purpose${new Date().getTime()}`;
    var dataResponsibleText = `dataResponsible${new Date().getTime()}`;
    var noteUsageText = `noteUsage${new Date().getTime()}`;
    var noteRiskText = `noteRiskText${new Date().getTime()}`;
    var urlNameText = `urlName${new Date().getTime()}`;


    beforeAll(() => {
        loginHelper.loginAsGlobalAdmin()
            .then(() => ItSystemCatalogHelper.createSystem(itSystem1))
            .then(() => ItSystemCatalogHelper.getActivationToggleButton(itSystem1).click())
            .then(() => ItSystemHelper.openLocalSystem(itSystem1));
    }, testFixture.longRunningSetup());

    beforeEach(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
        testFixture.cleanupState();
    });

    it("Fill out data on the GDPR page",
        () => {
            LocalSystemNavigation.openGDPRPage()
                .then(() => fillOutCheckboxes())
                .then(() => fillOutDropDown())
                .then(() => fillOutPrecautionsCheckboxes())
                .then(() => fillOutDateFields())
                .then(() => fillOutTextFields())
                .then(() => fillOutLinkFields())
                .then(() => ItSystemUsageGDPRPage.refreshPage())
                .then(() => verifyCheckBoxes())
                .then(() => verifyDropDown())
                .then(() => verifyDateFields())
                .then(() => verifyTextFields())
                .then(() => verifyLinkFields());
        });

    function fillOutCheckboxes() {
        console.log("Clicking on checkboxes");
        return ItSystemUsageGDPRPage.getSensitiveDataLevelCheckBox().click()
            .then(() => ItSystemUsageGDPRPage.getRegularDataLevelCheckBox().click())
            .then(() => ItSystemUsageGDPRPage.getLegalDataLevelCheckBox().click())
            .then(() => ItSystemUsageGDPRPage.getSensitiveDataOption1Checkbox().click())
            .then(() => browser.waitForAngular());

    }

    function fillOutPrecautionsCheckboxes() {
        return ItSystemUsageGDPRPage.getPrecautionsEncryptionCheckbox().click()
            .then(() => ItSystemUsageGDPRPage.getPrecautionsPseudonomiseringCheckbox().click())
            .then(() => ItSystemUsageGDPRPage.getPrecautionsAccessControlCheckbox().click())
            .then(() => ItSystemUsageGDPRPage.getPrecautionsLogningCheckbox().click())
            .then(() => browser.waitForAngular());
    }

    function fillOutDropDown() {
        console.log("Selecting values into the dropdown fields");
        return Select2Helper.selectWithNoSearch(dropdownYes, consts.gdprBusinessCriticalSelect2Id)
            .then(() => Select2Helper.selectWithNoSearch(dropdownYes, consts.gdprDPIASelect2Id))
            .then(() => Select2Helper.selectWithNoSearch(dropdownYes, consts.gdprAnsweringDataDPIASelect2Id))
            .then(() => Select2Helper.selectWithNoSearch(dropdownYes, consts.gdprRiskAssessmentSelect2Id))
            .then(() => Select2Helper.selectWithNoSearch(dropdownYes, consts.gdprPrecautionsSelect2Id))
            .then(() => Select2Helper.selectWithNoSearch(dropdownYes, consts.gdprsUserSupervisionSelect2Id))
            .then(() => Select2Helper.selectWithNoSearch(hostedValue, consts.hostedAtSelect2Id))
            .then(() => Select2Helper.selectWithNoSearch(preRiskAssessmentMiddle, consts.gdprPreRiskAssessment))
            .then(() => browser.waitForAngular());
    }

    function fillOutDateFields() {
        console.log("Entering a date into date fields");
        return ItSystemUsageGDPRPage.getRiskAssesmentDateField().sendKeys(dateValue)
            .then(() => ItSystemUsageGDPRPage.getDPIADateField().sendKeys(dateValue))
            .then(() => ItSystemUsageGDPRPage.getLatestRiskAssesmentDateField().sendKeys(dateValue))
            .then(() => ItSystemUsageGDPRPage.getDPIADeleteDateField().sendKeys(dateValue))
            .then(() => browser.waitForAngular());

    }

    function fillOutTextFields() {
        console.log("Entering data into text fields");
        return ItSystemUsageGDPRPage.getGDPRNumberDPIAField().sendKeys(protractor.Key.BACK_SPACE)
            .then(() => ItSystemUsageGDPRPage.getGDPRNumberDPIAField().sendKeys(numberValue))
            .then(() => ItSystemUsageGDPRPage.getGDPRSystemPurposeTextField().sendKeys(purposeText))
            .then(() => ItSystemUsageGDPRPage.getGDPRNoteRiskTextField().sendKeys(noteRiskText))
            .then(() => browser.waitForAngular());
    }

    function fillOutLinkFields() {
        console.log("Entering Urls into the link fields");
        return ItSystemUsageGDPRPage.getDPIALinkButton().click()
            .then(() => browser.waitForAngular())
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPRPage.getNoteLinkButton().click())
            .then(() => browser.waitForAngular())
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPRPage.getPrecautionLinkButton().click())
            .then(() => browser.waitForAngular())
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPRPage.getRiskLinkButton().click())
            .then(() => browser.waitForAngular())
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPRPage.getSuperVisionLinkButton().click())
            .then(() => browser.waitForAngular())
            .then(() => fillOutModalLinkWindow())
            .then(() => browser.waitForAngular());
    }

    function fillOutModalLinkWindow() {
        return ItSystemUsageGDPRPage.getModalNameField().clear()
            .then(() => ItSystemUsageGDPRPage.getModalNameField().sendKeys(urlNameText))
            .then(() => ItSystemUsageGDPRPage.getModalUrlField().clear())
            .then(() => ItSystemUsageGDPRPage.getModalUrlField().sendKeys(testUrl))
            .then(() => ItSystemUsageGDPRPage.getModalSaveButton().click())
            .then(() => browser.waitForAngular());
    }

    function verifyCheckBoxes() {
        console.log("Verifying check boxes");
        console.log("Sensitive boxes");
        expectCheckboxValue(consts.defaultSensitivData1, true);
        expectCheckboxValue(consts.defaultSensitivData2, false);
        console.log("Datalevel boxes");
        expectCheckboxValue(consts.dataLevelTypeNoneCheckbox, false);
        expectCheckboxValue(consts.dataLevelTypeRegularCheckbox, true);
        expectCheckboxValue(consts.dataLevelTypeSensitiveCheckbox, true);
        expectCheckboxValue(consts.dataLevelTypeLegalCheckbox, true);
        console.log("Precausion boxes");
        expectCheckboxValue(consts.precautionsEncryptionCheckbox, true);
        expectCheckboxValue(consts.precautionsPseudonomiseringCheckbox, true);
        expectCheckboxValue(consts.precautionsAccessControlCheckbox, true);
        expectCheckboxValue(consts.precautionsLogningCheckbox, true);
    }

    function verifyDropDown() {
        console.log("Verifying drop down");
        expectDropdownValueToEqual(dropdownYes, consts.gdprBusinessCriticalSelect2Id);
        expectDropdownValueToEqual(dropdownYes, consts.gdprRiskAssessmentSelect2Id);
        expectDropdownValueToEqual(dropdownYes, consts.gdprDPIASelect2Id);
        expectDropdownValueToEqual(dropdownYes, consts.gdprAnsweringDataDPIASelect2Id);
        expectDropdownValueToEqual(hostedValue, consts.hostedAtSelect2Id);
        expectDropdownValueToEqual(preRiskAssessmentMiddle, consts.gdprPreRiskAssessment);
    }

    function verifyDateFields() {
        console.log("Verifying date fields");
        expect(getValueAttribute(ItSystemUsageGDPRPage.getRiskAssesmentDateField())).toEqual(dateValue);
        expect(getValueAttribute(ItSystemUsageGDPRPage.getDPIADateField())).toEqual(dateValue);
        expect(getValueAttribute(ItSystemUsageGDPRPage.getDPIADeleteDateField())).toEqual(dateValue);
    }

    function verifyTextFields() {
        console.log("Verifying text fields");
        expect(getValueAttribute(ItSystemUsageGDPRPage.getGDPRSystemPurposeTextField())).toEqual(purposeText);
        expect(getValueAttribute(ItSystemUsageGDPRPage.getGDPRNoteRiskTextField())).toEqual(noteRiskText);
        expect(getValueAttribute(ItSystemUsageGDPRPage.getGDPRNumberDPIAField())).toEqual(numberValue);
    }

    function verifyLinkFields() {
        console.log("Verifying link fields");
        console.log("Checking Url");
        expect(getHrefUrl(ItSystemUsageGDPRPage.getNoteLinkField())).toEqual(testUrl);
        expect(getHrefUrl(ItSystemUsageGDPRPage.getPrecautionLinkField())).toEqual(testUrl);
        expect(getHrefUrl(ItSystemUsageGDPRPage.getSuperVisionLinkField())).toEqual(testUrl);
        expect(getHrefUrl(ItSystemUsageGDPRPage.getRiskLinkField())).toEqual(testUrl);
        expect(getHrefUrl(ItSystemUsageGDPRPage.getDPIALinkField())).toEqual(testUrl);
        console.log("Checking Name");
        expect(ItSystemUsageGDPRPage.getNoteLinkField().getText()).toEqual(urlNameText);
        expect(ItSystemUsageGDPRPage.getPrecautionLinkField().getText()).toEqual(urlNameText);
        expect(ItSystemUsageGDPRPage.getSuperVisionLinkField().getText()).toEqual(urlNameText);
        expect(ItSystemUsageGDPRPage.getRiskLinkField().getText()).toEqual(urlNameText);
        expect(ItSystemUsageGDPRPage.getDPIALinkField().getText()).toEqual(urlNameText);
    }

    function getHrefUrl(element: protractor.ElementFinder) {
        return element.getAttribute("href");
    }

    function getValueAttribute(element: protractor.ElementFinder) {
        return element.getAttribute("value");
    }

    function createItSystemName() {
        return `GdprTest${new Date().getTime()}`;
    }

    function expectCheckboxValue(checkBoxDataElementType: string, toBe: boolean) {
        console.log("Checking value for " + checkBoxDataElementType + " value to be " + toBe);
        return expect(element(cssHelper.byDataElementType(checkBoxDataElementType)).isSelected()).toBe(toBe);
    }

    function expectDropdownValueToEqual(expectedValue: string, idOfDropDownBox: string) {
        console.log("Expecting " + idOfDropDownBox + " to equal " + expectedValue);
        return expect(Select2Helper.getData(idOfDropDownBox).getText()).toEqual(expectedValue);
    }

    function getDate() {
        const currentDay = new Date().getDate();
        const currentMonth = new Date().getMonth() + 1; // getMonth gets counts 0. So January is 0.

        if (currentDay <= 9) {
            if (currentMonth <= 9) {
                return `0${currentDay}-0${currentMonth}-${new Date().getFullYear()}`;
            }
            return `0${currentDay}-${currentMonth}-${new Date().getFullYear()}`;
        }

        if (currentMonth <= 9) {
            return `${currentDay}-0${currentMonth}-${new Date().getFullYear()}`;
        }
        return `${currentDay}-${currentMonth}-${new Date().getFullYear()}`;

    }

});