import constants = require("../../../../Utility/Constants");
import CssHelper = require("../../../../Object-wrappers/CSSLocatorHelper");
import NavigationHelper = require("../../../../Utility/NavigationHelper")

class ItSystemUsageGDPR {

    private static consts = new constants();
    private static cssHelper = new CssHelper();
    private static navigationHelper = new NavigationHelper();

    static refreshPage(): webdriver.promise.Promise<void> {
        return ItSystemUsageGDPR.navigationHelper.refreshPage();
    }

    static getNoDataLevelCheckBox() {
        return element(this.cssHelper.byDataElementType(this.consts.dataLevelTypeNoneCheckbox));
    }

    static getRegularDataLevelCheckBox() {
        return element(this.cssHelper.byDataElementType(this.consts.dataLevelTypeRegularCheckbox));
    }

    static getSensitiveDataLevelCheckBox() {
        return element(this.cssHelper.byDataElementType(this.consts.dataLevelTypeSensitiveCheckbox));
    }

    static getLegalDataLevelCheckBox() {
        return element(this.cssHelper.byDataElementType(this.consts.dataLevelTypeLegalCheckbox));
    }

    static getPrecautionsEncryptionCheckbox() {
        return element(this.cssHelper.byDataElementType(this.consts.precautionsEncryptionCheckbox));
    }

    static getPrecautionsPseudonomiseringCheckbox() {
        return element(this.cssHelper.byDataElementType(this.consts.precautionsPseudonomiseringCheckbox));
    }

    static getPrecautionsAccessControlCheckbox() {
        return element(this.cssHelper.byDataElementType(this.consts.precautionsAccessControlCheckbox));
    }

    static getPrecautionsLogningCheckbox() {
        return element(this.cssHelper.byDataElementType(this.consts.precautionsLogningCheckbox));
    }

    static getSensitiveDataOption1Checkbox() {
        return element(this.cssHelper.byDataElementType(this.consts.defaultSensitivData1));
    }

    static getSensitiveDataOption2Checkbox() {
        return element(this.cssHelper.byDataElementType(this.consts.defaultSensitivData2));
    }

    static getGDPRSystemPurposeTextField() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprGDPRSystemPurposeTextField));
    } 

    static getGDPRNoteRiskTextField() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprNoteRiskTextField));
    }

    static getGDPRNumberDPIAField() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprNumberDPIATextField));
    }

    static getNoteLinkButton() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprNoteLinkButton));
    }

    static getPrecautionLinkButton() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprPrecautionLinkButton));
    }

    static getSuperVisionLinkButton() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprSuperVisionLinkButton));
    }

    static getRiskLinkButton() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprRiskLinkButton));
    }

    static getDPIALinkButton() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprDPIALinkButton));
    }

    static getNoteLinkField() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprNoteLinkField));
    }

    static getPrecautionLinkField() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprPrecautionLinkField));
    }

    static getSuperVisionLinkField() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprSuperVisionLinkField));
    }

    static getRiskLinkField() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprRiskLinkField));
    }

    static getDPIALinkField() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprDPIALinkField));
    }

    static getModalSaveButton() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprModalSaveLinkButton));
    }

    static getModalUrlField() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprModalUrlField));
    }

    static getModalNameField() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprModalNameField));
    }

    static getRiskAssesmentDateField() {
        return element(by.id(this.consts.gdprRiskAssesmentDateId));
    }

    static getDPIADateField() {
        return element(by.id(this.consts.gdprDPIADateForId));
    }

    static getDPIADeleteDateField() {
        return element(by.id(this.consts.gdprDPIAdeleteDateId));
    }

    static getLatestRiskAssesmentDateField() {
        return element(by.id(this.consts.gdprLatestRiskAssesmentDateDateId));
    }

    static getDataProcessingRegistrationView() {
        return element(this.cssHelper.byDataElementType(this.consts.gdprDataProcessingRegistrationView));
    }

    static getDataProcessingLink(dprName: string) {
        return element(by.linkText(dprName));
    }
}

export = ItSystemUsageGDPR;