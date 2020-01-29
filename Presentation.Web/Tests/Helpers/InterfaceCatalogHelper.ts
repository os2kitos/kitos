import InterfaceCatalogPage = require("../PageObjects/it-system/Interfaces/itSystemInterface.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import WaitTimers = require("../Utility/WaitTimers");
import Select2 = require("./Select2Helper");
import interfaceHelper = require("./InterfaceHelper");

class InterfaceCatalogHelper {
    private cssHelper = new CSSLocator();
    private waitUpTo = new WaitTimers();
    private interfacePage = new InterfaceCatalogPage();
    private interfaceHelper = new interfaceHelper();

    public createInterface(name: string) {
        console.log(`Creating interface with name ${name}`);
        return this.interfacePage.getPage()
            .then(() => this.interfacePage.waitForKendoGrid())
            .then(() => this.interfacePage.getCreateInterfaceButton().click())
            .then(() => expect(this.interfacePage.getInterfaceNameInputField().isPresent()))
            .then(() => this.interfacePage.getInterfaceNameInputField().sendKeys(name))
            .then(() => this.interfacePage.getSaveInterfaceButton().click());
    }

    public bindInterfaceToSystem(systemName: string, interfaceName: string) {
        console.log(`Binding interface with name ${interfaceName} to system with name ${systemName}`);
        return this.gotoSpecificInterface(interfaceName)
            .then(() => element(this.cssHelper.byDataElementType("interfaceDetailsLink")).click())
            .then(() => Select2.searchFor(systemName, "s2id_interface-exposed-by"))
            .then(() => Select2.waitForDataAndSelect());
    }

    public insertRandomDataToInterface(name: string, data: string, exposedBy: string, sysInterface: string, access: string, belongsTo: string, dataTypeTable: string) {
        console.log("Entering random data into a interface");
        return this.gotoSpecificInterface(name).then(() => {
            return this.interfaceHelper.writeDataToAllInputs(data, exposedBy, sysInterface, access, belongsTo, dataTypeTable);
        });
    }

    public verifyRandomDataToInterface(data: string, exposedBy: string, sysInterface: string, access: string, belongsTo: string, dataTypeTable: string) {
        console.log("Verifying random data in interface " + data);
        return this.gotoSpecificInterface(data).then(() => {
            return this.interfaceHelper.verifyDataWasSaved(data, exposedBy, sysInterface, access, belongsTo, dataTypeTable);
        });
    }

    public waitForKendoGrid() {
        console.log("Waiting for kendo grid to be ready");
        return browser.wait(this.interfacePage.waitForKendoGrid(), this.waitUpTo.twentySeconds);
    }

    public gotoSpecificInterface(name: string) {
        console.log(`Navigating to interface with name ${name}`);
        return this.interfacePage.getPage()
            .then(() => this.findSpecificInterfaceInNameColumn(name).click());
    }

    private findSpecificInterfaceInNameColumn(name: string) {
        console.log(`Finding interface with name : ${name}`);
        return element(by.xpath('//*/tbody/*/td/a[text()="' + name + '" and @data-element-type="InterfaceName"]'));
    }
}
export = InterfaceCatalogHelper;