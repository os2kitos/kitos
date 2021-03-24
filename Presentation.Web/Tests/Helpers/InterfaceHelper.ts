import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import Select2 = require("./Select2Helper");
import constant = require("../Utility/Constants");

class InterfaceHelper {
    private static const = new constant();
    private static cssHelper = new CSSLocator();


    public static writeDataToTextInput(data: string, ele: string) {
        console.log(`Writing ${data} To ${ele}`);
        return element(this.cssHelper.byDataElementType(ele)).clear().then(() => {
            return element(this.cssHelper.byDataElementType(ele)).sendKeys(data);
        });
    }

    public static writeDataToTable(data: string, dataType: string) {
        console.log(`Writing ${data} ${dataType} to table`);
        return element(this.cssHelper.byDataElementType(this.const.interfaceNewRowButton)).click().then(() => {
            return this.writeDataToTextInput(data, this.const.interfaceDataInput);
        }).then(() => {
            return this.selectDataFromSelect2Field(dataType, this.const.interfaceSelectTableDataType);
        });
    }

    public static verifyDataFromTextInput(data: string, ele: string) {
        console.log(`Verifying ${ele} Has value - ${data}`);
        expect(element(this.cssHelper.byDataElementType(ele)).isDisplayed()).toBe(true);
        expect(element(this.cssHelper.byDataElementType(ele)).getAttribute('value')).toEqual(data);
    }

    public static verifyDataFromSelect2(data: string, ele: string) {
        console.log(`Verifying ${ele} Has value - ${data}`);
        return expect(Select2.getData(ele).getText()).toEqual(data);
    }

    public static selectDataFromSelect2Field(search: string, id: string) {
        console.log(`Entering data into select2 field ${id}`);
        return Select2.searchFor(search, id).then(() => {
            console.log("Waiting for select2 data");
            return Select2.waitForDataAndSelect();
        });
    }

    public static selectDataFromNoSearchSelect2Field(search: string, id: string) {
        console.log(`Entering data into select2 field ${id}`);
        return Select2.selectWithNoSearch(search, id).then(() => {
            console.log("Waiting for select2 data");
            return Select2.waitForDataAndSelect();
        });
    }
}
export = InterfaceHelper