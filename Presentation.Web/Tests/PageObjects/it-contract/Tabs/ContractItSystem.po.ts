import constants = require("../../../Utility/Constants");
import CssHelper = require("../../../Object-wrappers/CSSLocatorHelper");

class ContractItSystem{
    private static consts = new constants();
    private static cssHelper = new CssHelper();


    public static getDescription(systemName: string) {
        return this.getElementFromTable(systemName, this.consts.relationDescriptionField);
    }

    public static getReference(systemName: string) {
        return this.getElementFromTable(systemName, this.consts.relationReferenceField);
    }

    public static getFrequencyType(systemName: string) {
        return this.getElementFromTable(systemName, this.consts.relationFrequencyTypeField);
    }

    public static getElementByLink(name: string) {
        return element(by.linkText(name));
    }

    private static getElementFromTable(rowLocator: string, columnLocator: string) {
        const rowElement = this.getElementByLink(rowLocator);
        const row = rowElement.element(by.xpath("../.."));
        return row.element(this.cssHelper.byDataElementType(columnLocator));
    }
}


export = ContractItSystem;