import constants = require("../../../../Utility/Constants");
import CssHelper = require("../../../../Object-wrappers/CSSLocatorHelper");

class ItSystemUsageCommon {

    private consts = new constants();
    private cssHelper = new CssHelper();
    private deleteButtonFinder;
    constructor() {
        this.deleteButtonFinder = new CssHelper().byDataElementType(this.consts.navigationRemoveSystemUsageButton);
    }

    getDeleteButton() {
        return element(this.deleteButtonFinder);
    }

    getDeleteButtons() {
        return element.all(this.deleteButtonFinder);
    }

    getSideNavigationItProject() {
        return element(by.css("[data-ui-sref='it-system.usage.proj']"));
    }
}

export = ItSystemUsageCommon;