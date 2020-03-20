import login = require("../../../Helpers/LoginHelper");
import ItSystemCatalogHelper = require("../../../Helpers/SystemCatalogHelper");
import itSystemHelper = require("../../../Helpers/SystemUsageHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import ItSystemUsageGDPR = require("../../../PageObjects/It-system/Usage/Tabs/ItSystemUsageGDPR.po");
import LocalSystemNavigation = require("../../../Helpers/SideNavigation/LocalItSystemNavigation");
import Constants = require("../../../Utility/Constants");
import CssHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import Select2Helper = require("../../../Helpers/Select2Helper");

describe("User is able to", () => {

    var loginHelper = new login();
    var testFixture = new TestFixtureWrapper();
    var itSystem1 = createItSystemName();
    var consts = new Constants();
    var cssHelper = new CssHelper();

    var dropDownValue = consts.gdprDefaultDropDownValueYes;
    var preRiskAssessmentValue = consts.gdprDefaultPreRiskAssessmentValue;
    var dateValue = consts.gdprDefaultDate;
    var testUrl = consts.gdprDefaultUrl;
    var defaultNumberValue = consts.gdprDefaultNumberText;
    var defaultText = consts.gdprDefaultText;


    beforeAll(() => {
        loginHelper.loginAsGlobalAdmin()
            .then(() => ItSystemCatalogHelper.createSystem(itSystem1))
            .then(() => ItSystemCatalogHelper.getActivationToggleButton(itSystem1).click())
            .then(() => itSystemHelper.openLocalSystem(itSystem1));
    }, testFixture.longRunningSetup());

    beforeEach(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
        testFixture.cleanupState();
    });

    it("Is able to fill out data on the GDPR page",
        () => {
            LocalSystemNavigation.openGDPRPage()
                .then(() => fillOutCheckboxes())
                .then(() => fillOutDropDown())
                .then(() => fillOutPrecautionsCheckboxes())
                .then(() => fillOutDateFields())
                .then(() => fillOutTextFields())
                .then(() => fillOutLinkFields())
                .then(() => ItSystemUsageGDPR.refreshPage())
                .then(() => verifyCheckBoxes())
                .then(() => verifyDropDown())
                .then(() => verifyDateFields())
                .then(() => verifyTextFields())
                .then(() => verifyLinkFields());
        });

    function fillOutCheckboxes() {
        console.log("Clicking on checkboxes");
        return ItSystemUsageGDPR.getSensitiveDataLevelCheckBox().click()
            .then(() => ItSystemUsageGDPR.getRegularDataLevelCheckBox().click())
            .then(() => ItSystemUsageGDPR.getLegalDataLevelCheckBox().click())
            .then(() => ItSystemUsageGDPR.getSensitiveTestDataCheckbox().click());

    }

    function fillOutPrecautionsCheckboxes() {
        return ItSystemUsageGDPR.getPrecautionsEncryptionCheckbox().click()
            .then(() => ItSystemUsageGDPR.getPrecautionsPseudonomiseringCheckbox().click())
            .then(() => ItSystemUsageGDPR.getPrecautionsAccessControlCheckbox().click())
            .then(() => ItSystemUsageGDPR.getPrecautionsLogningCheckbox().click());
    }

    function fillOutDropDown() {
        console.log("Selecting values into the dropdown fields");
        return selectValueFromSelect2Dropdown(dropDownValue, consts.gdprBusinessCriticalSelect2Id)
            .then(() => selectValueFromSelect2Dropdown(dropDownValue, consts.gdprDPIASelect2Id))
            .then(() => selectValueFromSelect2Dropdown(dropDownValue, consts.gdprAnsweringDataDPIASelect2Id))
            .then(() => selectValueFromSelect2Dropdown(dropDownValue, consts.gdprDataProcessorControlSelect2Id))
            .then(() => selectValueFromSelect2Dropdown(dropDownValue, consts.gdprRiskAssessmentSelect2Id))
            .then(() => selectValueFromSelect2Dropdown(dropDownValue, consts.gdprPrecautionsSelect2Id))
            .then(() => selectValueFromSelect2Dropdown(dropDownValue, consts.gdprsUserSupervisionSelect2Id))
            .then(() => selectValueFromSelect2Dropdown(preRiskAssessmentValue, consts.gdprPreRiskAssessment));
    }

    function fillOutDateFields() {
        console.log("Entering a date into date fields");
        return ItSystemUsageGDPR.getLastControlDateField().sendKeys(dateValue)
            .then(() => ItSystemUsageGDPR.getRiskAssesmentDateField().sendKeys(dateValue))
            .then(() => ItSystemUsageGDPR.getDPIADateField().sendKeys(dateValue))
            .then(() => ItSystemUsageGDPR.getLatestRiskAssesmentDateField().sendKeys(dateValue))
            .then(() => ItSystemUsageGDPR.getDPIADeleteDateField().sendKeys(dateValue));

    }

    function fillOutTextFields() {
        console.log("Entering data into text fields");
        return ItSystemUsageGDPR.getGDPRSystemPurposeTextField().sendKeys(defaultText)
            .then(() => ItSystemUsageGDPR.getGDPRDataResponsibleTextField().sendKeys(defaultText))
            .then(() => ItSystemUsageGDPR.getGDPRNoteUsageTextField().sendKeys(defaultText))
            .then(() => ItSystemUsageGDPR.getGDPRNoteRiskTextField().sendKeys(defaultText))
            .then(() => ItSystemUsageGDPR.getGDPRNumberDPIAField().clear())
            .then(() => ItSystemUsageGDPR.getGDPRNumberDPIAField().sendKeys(defaultNumberValue));
    }

    function fillOutLinkFields() {
        console.log("Entering Urls into the link fields");
        return ItSystemUsageGDPR.getDataProcessLinkButton().click()
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPR.getDPIALinkButton().click())
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPR.getNoteLinkButton().click())
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPR.getPrecautionLinkButton().click())
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPR.getRiskLinkButton().click())
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPR.getSuperVisionLinkButton().click())
            .then(() => fillOutModalLinkWindow());
    }

    function fillOutModalLinkWindow() {
        return ItSystemUsageGDPR.getModalNameField().clear()
            .then(() => ItSystemUsageGDPR.getModalNameField().sendKeys(defaultText))
            .then(() => ItSystemUsageGDPR.getModalUrlField().clear())
            .then(() => ItSystemUsageGDPR.getModalUrlField().sendKeys(testUrl))
            .then(() => ItSystemUsageGDPR.getModalSaveButton().click());
    }

    function verifyCheckBoxes() {
        console.log("Verifying check boxes");
        console.log("Sensitive boxes");
        expectCheckboxValue(consts.defaultPersonalSensitivData1, true);
        expectCheckboxValue(consts.defaultPersonalSensitivData2, false);
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
        expectDropdownValueToEqual(dropDownValue, consts.gdprBusinessCriticalSelect2Id);
        expectDropdownValueToEqual(dropDownValue, consts.gdprDataProcessorControlSelect2Id);
        expectDropdownValueToEqual(dropDownValue, consts.gdprRiskAssessmentSelect2Id);
        expectDropdownValueToEqual(dropDownValue, consts.gdprDPIASelect2Id);
        expectDropdownValueToEqual(dropDownValue, consts.gdprAnsweringDataDPIASelect2Id);
        expectDropdownValueToEqual(preRiskAssessmentValue, consts.gdprPreRiskAssessment);
    }

    function verifyDateFields() {
        console.log("Verifying date fields");
        expect(ItSystemUsageGDPR.getLastControlDateField().getAttribute("value")).toEqual(dateValue);
        expect(ItSystemUsageGDPR.getRiskAssesmentDateField().getAttribute("value")).toEqual(dateValue);
        expect(ItSystemUsageGDPR.getDPIADateField().getAttribute("value")).toEqual(dateValue);
        expect(ItSystemUsageGDPR.getDPIADeleteDateField().getAttribute("value")).toEqual(dateValue);
    }

    function verifyTextFields() {
        console.log("Verifying text fields");
        expect(ItSystemUsageGDPR.getGDPRSystemPurposeTextField().getAttribute("value")).toEqual(defaultText);
        expect(ItSystemUsageGDPR.getGDPRDataResponsibleTextField().getAttribute("value")).toEqual(defaultText);
        expect(ItSystemUsageGDPR.getGDPRNoteUsageTextField().getAttribute("value")).toEqual(defaultText);
        expect(ItSystemUsageGDPR.getGDPRNoteRiskTextField().getAttribute("value")).toEqual(defaultText);
        expect(ItSystemUsageGDPR.getGDPRNumberDPIAField().getAttribute("value")).toEqual(defaultNumberValue);
    }

    function verifyLinkFields() {
        console.log("Verifying link fields");
        console.log("Checking Url");
        expect(ItSystemUsageGDPR.getDataProcessLinkField().getAttribute("href")).toEqual(testUrl);
        expect(ItSystemUsageGDPR.getNoteLinkField().getAttribute("href")).toEqual(testUrl);
        expect(ItSystemUsageGDPR.getPrecautionLinkField().getAttribute("href")).toEqual(testUrl);
        expect(ItSystemUsageGDPR.getSuperVisionLinkField().getAttribute("href")).toEqual(testUrl);
        expect(ItSystemUsageGDPR.getRiskLinkField().getAttribute("href")).toEqual(testUrl);
        expect(ItSystemUsageGDPR.getDPIALinkField().getAttribute("href")).toEqual(testUrl);
        console.log("Checking Name");
        expect(ItSystemUsageGDPR.getDataProcessLinkField().getText()).toEqual(defaultText);
        expect(ItSystemUsageGDPR.getNoteLinkField().getText()).toEqual(defaultText);
        expect(ItSystemUsageGDPR.getPrecautionLinkField().getText()).toEqual(defaultText);
        expect(ItSystemUsageGDPR.getSuperVisionLinkField().getText()).toEqual(defaultText);
        expect(ItSystemUsageGDPR.getRiskLinkField().getText()).toEqual(defaultText);
        expect(ItSystemUsageGDPR.getDPIALinkField().getText()).toEqual(defaultText);
    }


    function createItSystemName() {
        return `SystemUsageMain${new Date().getTime()}`;
    }

    function expectCheckboxValue(checkBoxDataElementType: string, toBe: boolean) {
        console.log("Checking value for " + checkBoxDataElementType + " value to be " + toBe);
        return expect(element(cssHelper.byDataElementType(checkBoxDataElementType)).isSelected()).toBe(toBe);
    }

    function selectValueFromSelect2Dropdown(valueToSelect: string, idOfDropDownBox: string) {
        return Select2Helper.selectWithNoSearch(valueToSelect, idOfDropDownBox);
    }

    function expectDropdownValueToEqual(expectedValue: string, idOfDropDownBox: string) {
        console.log("Expecting " + idOfDropDownBox + " to equal " + expectedValue);
        return expect(Select2Helper.getData(idOfDropDownBox).getText()).toEqual(expectedValue);
    }

});