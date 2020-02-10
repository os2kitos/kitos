import constants = require("../../../../Utility/Constants");
import CssHelper = require("../../../../Object-wrappers/CSSLocatorHelper");

class ItSystemUsageRelation {

    private static consts = new constants();
    private static cssHelper = new CssHelper();

    public static getCreateButton() {
        return element(by.id(this.consts.createRelationButton));
    }

    public static getReferenceInputField() {
        return element(by.id(this.consts.referenceInputField));
    }

    public static getDescriptionInputField() {
        return element(by.id(this.consts.descriptionInputField));
    }
    
    public static getSaveButton() {
        return element(this.cssHelper.byDataElementType("relationSaveButton"));
    }

    public static getRelationExhibitSystem(systemName: string) {
        return element(by.linkText(systemName));
    }

    public static getRelationInterface(interfaceName: string) {
        return element(by.linkText(interfaceName));
    }

    public static getRelationContract(interfaceName: string) {
        return element(by.linkText(interfaceName));
    }

}

export = ItSystemUsageRelation;