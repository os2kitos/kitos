﻿import constants = require("../../../../Utility/Constants");
import CssHelper = require("../../../../Object-wrappers/CSSLocatorHelper");
import NavigationHelper = require("../../../../Utility/NavigationHelper")

class ItSystemUsageDataProcessing {

    private static consts = new constants();
    private static cssHelper = new CssHelper();
    private static navigationHelper = new NavigationHelper();

    static refreshPage(): webdriver.promise.Promise<void> {
        return ItSystemUsageDataProcessing.navigationHelper.refreshPage();
    }

    static getDataProcessingRegistrationView() {
        console.log("Getting dpr view element");
        return element(this.cssHelper.byDataElementType(this.consts.dataProcessingRegistrationView));
    }

    static getDataProcessingLink(dprName: string) {
        return element(by.linkText(dprName));
    }
}

export = ItSystemUsageDataProcessing;