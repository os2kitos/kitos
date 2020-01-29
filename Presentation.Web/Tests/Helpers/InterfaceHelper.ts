import InterfaceCatalogPage = require("../PageObjects/it-system/Interfaces/itSystemInterface.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import WaitTimers = require("../Utility/WaitTimers");
import Select2 = require("./Select2Helper");
import constant = require("../Utility/Constants")

class InterfaceHelper {
    private const = new constant();
    private cssHelper = new CSSLocator();


    public writeDataToAllInputs(data: string, exposedBy: string, sysInterface: string, access: string, belongsTo: string, dataTypeTable: string) {
        console.log("Writing data to field: " + this.const.interfaceNameInput);
        return this.writeDataToTextInput(data, this.const.interfaceNameInput)
            .then(() => {
                console.log("Writing data to field: " + this.const.interfaceIdInput);
                return this.writeDataToTextInput(data, this.const.interfaceIdInput);
            }).then(() => {
                console.log("Writing data to field: " + this.const.interfaceVersionInput);
                return this.writeDataToTextInput(data, this.const.interfaceVersionInput);
            }).then(() => {
                console.log("Writing data to field: " + this.const.interfaceDescriptionInput);
                return this.writeDataToTextInput(data, this.const.interfaceDescriptionInput);
            }).then(() => {
                console.log("Writing data to field: " + this.const.interfaceDescriptionLinkInput);
                return this.writeDataToTextInput(data, this.const.interfaceDescriptionLinkInput);
            }).then(() => {
                console.log("Writing data to field: " + this.const.interfaceNoteInput);
                return this.writeDataToTextInput(data, this.const.interfaceNoteInput);
            }).then(() => {
                console.log("Entering data into select2 field");
                return this.selectDataFromSelect2Field(exposedBy, this.const.interfaceSelectExhibit);
            }).then(() => {
                console.log("Entering data into select2 field");
                return this.selectDataFromSelect2Field(sysInterface, this.const.interfaceSelectInterface);
            }).then(() => {
                console.log("Entering data into select2 field");
                return this.selectDataFromSelect2Field(access, this.const.interfaceSelectAccess);
            }).then(() => {
                console.log("Entering data into select2 field");
                return this.selectDataFromSelect2Field(belongsTo, this.const.interfaceSelectBelongs);
            }).then(() => {
                return this.writeDataToTable(data, dataTypeTable);
            });
    }


    public verifyDataWasSaved(data: string, exposedBy: string, sysInterface: string, access: string, belongsTo: string, dataTypeTable: string) {
        // text 
        this.verifyDataFromTextInput(data, this.const.interfaceNameInput);
        this.verifyDataFromTextInput(data, this.const.interfaceIdInput);
        this.verifyDataFromTextInput(data, this.const.interfaceVersionInput);
        this.verifyDataFromTextInput(data, this.const.interfaceDescriptionInput);
        this.verifyDataFromTextInput(data, this.const.interfaceDescriptionLinkInput);
        this.verifyDataFromTextInput(data, this.const.interfaceNoteInput);
        this.verifyDataFromTextInput(data, this.const.interfaceDataInput);

        //select2
        this.verifyDataFromSelect2(exposedBy, this.const.interfaceSelectExhibit);
        this.verifyDataFromSelect2(sysInterface, this.const.interfaceSelectInterface);
        this.verifyDataFromSelect2(access, this.const.interfaceSelectAccess);
        this.verifyDataFromSelect2(belongsTo, this.const.interfaceSelectBelongs);
        this.verifyDataFromSelect2(dataTypeTable, this.const.interfaceSelectTableDataType);

    }


    private writeDataToTextInput(data: string, ele: string) {
        console.log("Writing " + data + " To " + ele);
        return element(this.cssHelper.byDataElementType(ele)).clear().then(() => {
            return element(this.cssHelper.byDataElementType(ele)).sendKeys(data);
        });
    }

    private writeDataToTable(data: string, dataType: string) {
        console.log("Writing " + data + " " + dataType + " to table");
        return element(this.cssHelper.byDataElementType(this.const.interfaceNewRowButton)).click().then(() => {
            return this.writeDataToTextInput(data, this.const.interfaceDataInput);
        }).then(() => {
            return this.selectDataFromSelect2Field(dataType, this.const.interfaceSelectTableDataType);
        });
    }

    private verifyDataFromTextInput(data: string, ele: string) {
        console.log("Verifying " + ele + " Has value - " + data);
        expect(element(this.cssHelper.byDataElementType(ele)).isDisplayed()).toBe(true);
        expect(element(this.cssHelper.byDataElementType(ele)).getAttribute('value')).toEqual(data);
    }

    private verifyDataFromSelect2(data: string, ele: string) {
        console.log("Verifying " + ele + " Has value - " + data);
        return expect(Select2.getData(ele).getText()).toEqual(data);
    }

    private selectDataFromSelect2Field(search: string, id: string) {
        console.log("Entering data into select2 field " + id);
        return Select2.searchFor(search, id).then(() => {
            console.log("Waiting for select2 data");
            return Select2.waitForDataAndSelect();
        });
    }

}
export = InterfaceHelper