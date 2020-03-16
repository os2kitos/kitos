import constants = require("../../../../Utility/Constants");
import CssHelper = require("../../../../Object-wrappers/CSSLocatorHelper");

class ItSystemUsageGDPR {

    private static consts = new constants();
    private static cssHelper = new CssHelper();

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

    static getDefaultPersonalSensitiveDataCheckbox1() {
        return element(this.cssHelper.byDataElementType(this.consts.defaultPersonalSensitivData1));
    }

    static getDefaultPersonalSensitiveDataCheckbox2() {
        return element(this.cssHelper.byDataElementType(this.consts.defaultPersonalSensitivData2));
    }
}

export = ItSystemUsageGDPR;