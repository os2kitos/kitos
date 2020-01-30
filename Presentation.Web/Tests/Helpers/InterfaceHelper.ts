import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import Select2 = require("./Select2Helper");
import constant = require("../Utility/Constants")

class InterfaceHelper {
    private const = new constant();s
    private cssHelper = new CSSLocator();

    public writeDataToAllInputs(data: string, exposedBy: string, sysInterface: string, access: string, belongsTo: string, dataTypeTable: string) {
        return this.writeDataToTextInput(data, this.const.interfaceNameInput)
            .then(() => {
                return this.writeDataToTextInput(data, this.const.interfaceIdInput);
            }).then(() => {
                return this.writeDataToTextInput(data, this.const.interfaceVersionInput);
            }).then(() => {
                return this.writeDataToTextInput(data, this.const.interfaceDescriptionInput);
            }).then(() => {
                return this.writeDataToTextInput(data, this.const.interfaceDescriptionLinkInput);
            }).then(() => {
                return this.writeDataToTextInput(data, this.const.interfaceNoteInput);
            }).then(() => {
                return this.selectDataFromSelect2Field(exposedBy, this.const.interfaceSelectExhibit);
            }).then(() => {
                return this.selectDataFromSelect2Field(sysInterface, this.const.interfaceSelectInterface);
            }).then(() => {
                return this.selectDataFromSelect2Field(access, this.const.interfaceSelectAccess);
            }).then(() => {
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