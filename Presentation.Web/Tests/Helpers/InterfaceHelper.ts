import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import Select2 = require("./Select2Helper");
import constant = require("../Utility/Constants")
import interfaceCatalogHelper = require("./InterfaceCatalogHelper");

class InterfaceHelper {
    private static const = new constant();
    private static cssHelper = new CSSLocator();

    public static writeDataToAllInputs(nameOfInterface: string, data: string, exposedBy: string, sysInterface: string, access: string, belongsTo: string, dataTypeTable: string, version: string) {
        return interfaceCatalogHelper.gotoSpecificInterface(nameOfInterface).then(() =>
            this.writeDataToTextInput(data, this.const.interfaceNameInput)
                .then(() => {
                    return this.writeDataToTextInput(data + "1", this.const.interfaceIdInput);
                }).then(() => {
                    return this.writeDataToTextInput(data + "2", this.const.interfaceDescriptionInput);
                }).then(() => {
                    return this.writeDataToTextInput(data + "3", this.const.interfaceDescriptionLinkInput);
                }).then(() => {
                    return this.writeDataToTextInput(data + "4", this.const.interfaceNoteInput);
                }).then(() => {
                    return this.selectDataFromSelect2Field(exposedBy, this.const.interfaceSelectExhibit);
                }).then(() => {
                    return this.selectDataFromSelect2Field(sysInterface, this.const.interfaceSelectInterface);
                }).then(() => {
                    return this.selectDataFromSelect2Field(access, this.const.interfaceSelectAccess);
                }).then(() => {
                    return this.selectDataFromSelect2Field(belongsTo, this.const.interfaceSelectBelongs);
                }).then(() => {
                    return this.writeDataToTextInput(version, this.const.interfaceVersionInput);
                }).then(() => {
                    return this.writeDataToTable(data, dataTypeTable);
                }));
    }

    public static verifyDataWasSaved(data: string, exposedBy: string, sysInterface: string, access: string, belongsTo: string, dataTypeTable: string, version: string) {
        return interfaceCatalogHelper.gotoSpecificInterface(data).then(() => {
            // text 
            this.verifyDataFromTextInput(data, this.const.interfaceNameInput);
            this.verifyDataFromTextInput(data + "1", this.const.interfaceIdInput);
            this.verifyDataFromTextInput(data + "2", this.const.interfaceDescriptionInput);
            this.verifyDataFromTextInput(data + "3", this.const.interfaceDescriptionLinkInput);
            this.verifyDataFromTextInput(data + "4", this.const.interfaceNoteInput);
            this.verifyDataFromTextInput(data, this.const.interfaceDataInput);
            this.verifyDataFromTextInput(version, this.const.interfaceVersionInput);

            //select2
            this.verifyDataFromSelect2(exposedBy, this.const.interfaceSelectExhibit);
            this.verifyDataFromSelect2(sysInterface, this.const.interfaceSelectInterface);
            this.verifyDataFromSelect2(access, this.const.interfaceSelectAccess);
            this.verifyDataFromSelect2(belongsTo, this.const.interfaceSelectBelongs);
            this.verifyDataFromSelect2(dataTypeTable, this.const.interfaceSelectTableDataType);
        });
    }

    private static writeDataToTextInput(data: string, ele: string) {
        console.log(`Writing ${data} To ${ele}`);
        return element(this.cssHelper.byDataElementType(ele)).clear().then(() => {
            return element(this.cssHelper.byDataElementType(ele)).sendKeys(data);
        });
    }

    private static writeDataToTable(data: string, dataType: string) {
        console.log(`Writing ${data} ${dataType} to table`);
        return element(this.cssHelper.byDataElementType(this.const.interfaceNewRowButton)).click().then(() => {
            return this.writeDataToTextInput(data, this.const.interfaceDataInput);
        }).then(() => {
            return this.selectDataFromSelect2Field(dataType, this.const.interfaceSelectTableDataType);
        });
    }

    private static verifyDataFromTextInput(data: string, ele: string) {
        console.log(`Verifying ${ele} Has value - ${data}`);
        expect(element(this.cssHelper.byDataElementType(ele)).isDisplayed()).toBe(true);
        expect(element(this.cssHelper.byDataElementType(ele)).getAttribute('value')).toEqual(data);
    }

    private static verifyDataFromSelect2(data: string, ele: string) {
        console.log(`Verifying ${ele} Has value - ${data}`);
        return expect(Select2.getData(ele).getText()).toEqual(data);
    }

    private static selectDataFromSelect2Field(search: string, id: string) {
        console.log(`Entering data into select2 field ${id}`);
        return Select2.searchFor(search, id).then(() => {
            console.log("Waiting for select2 data");
            return Select2.waitForDataAndSelect();
        });
    }
}
export = InterfaceHelper