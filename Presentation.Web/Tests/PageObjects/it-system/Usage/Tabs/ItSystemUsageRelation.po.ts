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

    public static getRelationLink(name: string) {
        return element(by.linkText(name));
    }

    public static getDescription(systemName: string) {
        return this.getElementFromTable(systemName, this.consts.relationDescriptionField);
    }

    public static getReference(systemName: string) {
        return this.getElementFromTable(systemName, this.consts.relationReferenceField);
    }

    public static getFrequencyType(systemName: string) {
        return this.getElementFromTable(systemName, this.consts.relationFrequencyTypeField);
    }

    public static getUsedByDescription(systemName: string) {
        return this.getElementFromTable(systemName, this.consts.usedByRelationDescriptionField);
    }

    public static getUsedByReference(systemName: string) {
        return this.getElementFromTable(systemName, this.consts.usedByRelationReferenceField);
    }

    public static getUsedByFrequencyType(systemName: string) {
        return this.getElementFromTable(systemName, this.consts.usedByRelationFrequencyTypeField);
    }

    private static getElementFromTable(rowLocator: string, columnLocator: string) {
        const rowElement = this.getRelationLink(rowLocator);
        const row = rowElement.element(by.xpath("../.."));
        return row.element(this.cssHelper.byDataElementType(columnLocator));
    }

}

export = ItSystemUsageRelation;