﻿import constants = require("../../../../Utility/Constants");
import CssHelper = require("../../../../Object-wrappers/CSSLocatorHelper");

class ItSystemUsageMain {

    private static consts = new constants();
    private static cssHelper = new CssHelper();

    static getHeaderName() {
        return element(by.id(this.consts.systemUsageHeaderName));
    }

    static getLocalId() {
        return element(by.id(this.consts.mainLocalId));
    }

    static getLocalCallName() {
        return element(by.id(this.consts.mainCallName));
    }

    static getNote() {
        return element(by.id(this.consts.mainNote));
    }

    static getVersion() {
        return element(by.id(this.consts.mainVersion));
    }

    static getOwner() {
        return element(by.id(this.consts.mainOwner));
    }

    static getSystemName() {
        return element(by.id(this.consts.mainSystemName));
    }

    static getParentName() {
        return element(by.id(this.consts.mainParentName));
    }

    static getPreviousName() {
        return element(by.id(this.consts.mainPreviousName));
    }

    static getBelongsTo() {
        return element(by.id(this.consts.mainBelongsTo));
    }

    static getAccessModifier() {
        return element(by.id(this.consts.mainAccess));
    }

    static getDescription() {
        return element(by.id(this.consts.mainDescription));
    }

    static getBusinessType() {
        return element(by.id(this.consts.mainBusinessType));
    }

    static getArchiveDuty() {
        return element(by.id(this.consts.mainArchive));
    }

    static getUUID() {
        return element(by.id(this.consts.mainUUID));
    }


    

}

export = ItSystemUsageMain;