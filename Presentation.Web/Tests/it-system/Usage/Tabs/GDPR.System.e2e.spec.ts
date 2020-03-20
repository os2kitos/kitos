import login = require("../../../Helpers/LoginHelper");
import ItSystemCatalogHelper = require("../../../Helpers/SystemCatalogHelper");
import itSystemHelper = require("../../../Helpers/SystemUsageHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import ItSystemUsageGDPRPage = require("../../../PageObjects/It-system/Usage/Tabs/ItSystemUsageGDPR.po");
import LocalSystemNavigation = require("../../../Helpers/SideNavigation/LocalItSystemNavigation");
import Constants = require("../../../Utility/Constants");
import CssHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import Select2Helper = require("../../../Helpers/Select2Helper");

describe("Global admin is able to", () => {

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
            .then(() => ItSystemUsageGDPRPage.getSensitiveTestDataCheckbox().click());

    }

    function fillOutPrecautionsCheckboxes() {
        return ItSystemUsageGDPRPage.getPrecautionsEncryptionCheckbox().click()
            .then(() => ItSystemUsageGDPRPage.getPrecautionsPseudonomiseringCheckbox().click())
            .then(() => ItSystemUsageGDPRPage.getPrecautionsAccessControlCheckbox().click())
            .then(() => ItSystemUsageGDPRPage.getPrecautionsLogningCheckbox().click());
    }

    function fillOutDropDown() {
        console.log("Selecting values into the dropdown fields");
        return Select2Helper.selectWithNoSearch(dropDownValue, consts.gdprBusinessCriticalSelect2Id)
            .then(() => Select2Helper.selectWithNoSearch(dropDownValue, consts.gdprDPIASelect2Id))
            .then(() => Select2Helper.selectWithNoSearch(dropDownValue, consts.gdprAnsweringDataDPIASelect2Id))
            .then(() => Select2Helper.selectWithNoSearch(dropDownValue, consts.gdprDataProcessorControlSelect2Id))
            .then(() => Select2Helper.selectWithNoSearch(dropDownValue, consts.gdprRiskAssessmentSelect2Id))
            .then(() => Select2Helper.selectWithNoSearch(dropDownValue, consts.gdprPrecautionsSelect2Id))
            .then(() => Select2Helper.selectWithNoSearch(dropDownValue, consts.gdprsUserSupervisionSelect2Id))
            .then(() => Select2Helper.selectWithNoSearch(preRiskAssessmentValue, consts.gdprPreRiskAssessment));
    }

    function fillOutDateFields() {
        console.log("Entering a date into date fields");
        return ItSystemUsageGDPRPage.getLastControlDateField().sendKeys(dateValue)
            .then(() => ItSystemUsageGDPRPage.getRiskAssesmentDateField().sendKeys(dateValue))
            .then(() => ItSystemUsageGDPRPage.getDPIADateField().sendKeys(dateValue))
            .then(() => ItSystemUsageGDPRPage.getLatestRiskAssesmentDateField().sendKeys(dateValue))
            .then(() => ItSystemUsageGDPRPage.getDPIADeleteDateField().sendKeys(dateValue));

    }

    function fillOutTextFields() {
        console.log("Entering data into text fields");
        return ItSystemUsageGDPRPage.getGDPRSystemPurposeTextField().sendKeys(defaultText)
            .then(() => ItSystemUsageGDPRPage.getGDPRDataResponsibleTextField().sendKeys(defaultText))
            .then(() => ItSystemUsageGDPRPage.getGDPRNoteUsageTextField().sendKeys(defaultText))
            .then(() => ItSystemUsageGDPRPage.getGDPRNoteRiskTextField().sendKeys(defaultText))
            .then(() => ItSystemUsageGDPRPage.getGDPRNumberDPIAField().clear())
            .then(() => ItSystemUsageGDPRPage.getGDPRNumberDPIAField().sendKeys(defaultNumberValue));
    }

    function fillOutLinkFields() {
        console.log("Entering Urls into the link fields");
        return ItSystemUsageGDPRPage.getDataProcessLinkButton().click()
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPRPage.getDPIALinkButton().click())
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPRPage.getNoteLinkButton().click())
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPRPage.getPrecautionLinkButton().click())
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPRPage.getRiskLinkButton().click())
            .then(() => fillOutModalLinkWindow())
            .then(() => ItSystemUsageGDPRPage.getSuperVisionLinkButton().click())
            .then(() => fillOutModalLinkWindow());
    }

    function fillOutModalLinkWindow() {
        return ItSystemUsageGDPRPage.getModalNameField().clear()
            .then(() => ItSystemUsageGDPRPage.getModalNameField().sendKeys(defaultText))
            .then(() => ItSystemUsageGDPRPage.getModalUrlField().clear())
            .then(() => ItSystemUsageGDPRPage.getModalUrlField().sendKeys(testUrl))
            .then(() => ItSystemUsageGDPRPage.getModalSaveButton().click());
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
        expectDropdownValueToEqual(dropDownValue, consts.gdprBusinessCriticalSelect2Id);
        expectDropdownValueToEqual(dropDownValue, consts.gdprDataProcessorControlSelect2Id);
        expectDropdownValueToEqual(dropDownValue, consts.gdprRiskAssessmentSelect2Id);
        expectDropdownValueToEqual(dropDownValue, consts.gdprDPIASelect2Id);
        expectDropdownValueToEqual(dropDownValue, consts.gdprAnsweringDataDPIASelect2Id);
        expectDropdownValueToEqual(preRiskAssessmentValue, consts.gdprPreRiskAssessment);
    }

    function verifyDateFields() {
        console.log("Verifying date fields");
        expect(ItSystemUsageGDPRPage.getLastControlDateField().getAttribute("value")).toEqual(dateValue);
        expect(ItSystemUsageGDPRPage.getRiskAssesmentDateField().getAttribute("value")).toEqual(dateValue);
        expect(ItSystemUsageGDPRPage.getDPIADateField().getAttribute("value")).toEqual(dateValue);
        expect(ItSystemUsageGDPRPage.getDPIADeleteDateField().getAttribute("value")).toEqual(dateValue);
    }

    function verifyTextFields() {
        console.log("Verifying text fields");
        expect(ItSystemUsageGDPRPage.getGDPRSystemPurposeTextField().getAttribute("value")).toEqual(defaultText);
        expect(ItSystemUsageGDPRPage.getGDPRDataResponsibleTextField().getAttribute("value")).toEqual(defaultText);
        expect(ItSystemUsageGDPRPage.getGDPRNoteUsageTextField().getAttribute("value")).toEqual(defaultText);
        expect(ItSystemUsageGDPRPage.getGDPRNoteRiskTextField().getAttribute("value")).toEqual(defaultText);
        expect(ItSystemUsageGDPRPage.getGDPRNumberDPIAField().getAttribute("value")).toEqual(defaultNumberValue);
    }

    function verifyLinkFields() {
        console.log("Verifying link fields");
        console.log("Checking Url");
        expect(ItSystemUsageGDPRPage.getDataProcessLinkField().getAttribute("href")).toEqual(testUrl);
        expect(ItSystemUsageGDPRPage.getNoteLinkField().getAttribute("href")).toEqual(testUrl);
        expect(ItSystemUsageGDPRPage.getPrecautionLinkField().getAttribute("href")).toEqual(testUrl);
        expect(ItSystemUsageGDPRPage.getSuperVisionLinkField().getAttribute("href")).toEqual(testUrl);
        expect(ItSystemUsageGDPRPage.getRiskLinkField().getAttribute("href")).toEqual(testUrl);
        expect(ItSystemUsageGDPRPage.getDPIALinkField().getAttribute("href")).toEqual(testUrl);
        console.log("Checking Name");
        expect(ItSystemUsageGDPRPage.getDataProcessLinkField().getText()).toEqual(defaultText);
        expect(ItSystemUsageGDPRPage.getNoteLinkField().getText()).toEqual(defaultText);
        expect(ItSystemUsageGDPRPage.getPrecautionLinkField().getText()).toEqual(defaultText);
        expect(ItSystemUsageGDPRPage.getSuperVisionLinkField().getText()).toEqual(defaultText);
        expect(ItSystemUsageGDPRPage.getRiskLinkField().getText()).toEqual(defaultText);
        expect(ItSystemUsageGDPRPage.getDPIALinkField().getText()).toEqual(defaultText);
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

});