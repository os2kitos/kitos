import InterfaceCatalogPage = require("../PageObjects/it-system/Interfaces/itSystemInterface.po");
import WaitTimers = require("../Utility/WaitTimers");
import Select2 = require("./Select2Helper");
import interfaceHelper = require("./InterfaceHelper");

class InterfaceCatalogHelper {
    private static waitUpTo = new WaitTimers();
    private static interfacePage = new InterfaceCatalogPage();
    private static interfaceHelper = new interfaceHelper();

    public static createInterface(name: string) {
        console.log(`Creating interface with name ${name}`);
        return this.interfacePage.getPage()
            .then(() => this.interfacePage.waitForKendoGrid())
            .then(() => this.interfacePage.getCreateInterfaceButton().click())
            .then(() => expect(this.interfacePage.getInterfaceNameInputField().isPresent()))
            .then(() => this.interfacePage.getInterfaceNameInputField().sendKeys(name))
            .then(() => this.interfacePage.getSaveInterfaceButton().click())
            .then(() => console.log("Interface created"));;
    }

    public static bindInterfaceToSystem(systemName: string, interfaceName: string) {
        console.log(`Binding interface with name ${interfaceName} to system with name ${systemName}`);
        return this.gotoSpecificInterface(interfaceName)
            .then(() => Select2.searchFor(systemName, "s2id_interface-exposed-by"))
            .then(() => Select2.waitForDataAndSelect())
            .then(() => console.log("Interface bound to system"));;
    }

    public static insertRandomDataToInterface(name: string, data: string, exposedBy: string, sysInterface: string, access: string, belongsTo: string, dataTypeTable: string) {
        console.log("Entering random data into a interface");
        return this.gotoSpecificInterface(name).then(() => {
            return this.interfaceHelper.writeDataToAllInputs(data, exposedBy, sysInterface, access, belongsTo, dataTypeTable);
        });
    }

    public static verifyRandomDataToInterface(data: string, exposedBy: string, sysInterface: string, access: string, belongsTo: string, dataTypeTable: string) {
        console.log("Verifying random data in interface " + data);
        return this.gotoSpecificInterface(data).then(() => {
            return this.interfaceHelper.verifyDataWasSaved(data, exposedBy, sysInterface, access, belongsTo, dataTypeTable);
        });
    }
    
    public static waitForKendoGrid() {
        console.log("Waiting for kendo grid to be ready");
        return browser.wait(this.interfacePage.waitForKendoGrid(), this.waitUpTo.twentySeconds);
    }

    private static gotoSpecificInterface(name : string) {
        console.log(`Navigating to interface with name ${name}`);
        return this.interfacePage.getPage()
            .then(() => this.findSpecificInterfaceInNameColumn(name).click());
    }

    private static findSpecificInterfaceInNameColumn(name: string) {
        console.log(`Finding interface with name : ${name}`);
        return element(by.xpath('//*/tbody/*/td/a[text()="' + name + '" and @data-element-type="InterfaceName"]'));
    }
}
export = InterfaceCatalogHelper;