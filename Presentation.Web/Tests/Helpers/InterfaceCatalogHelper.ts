import InterfaceCatalogPage = require("../PageObjects/it-system/Interfaces/itSystemInterface.po");
import Select2 = require("./Select2Helper");

class InterfaceCatalogHelper {
    private static interfacePage = new InterfaceCatalogPage();

    public static createInterface(name: string) {
        console.log(`Creating interface with name ${name}`);
        return this.interfacePage.getPage()
            .then(() => this.interfacePage.getCreateInterfaceButton().click())
            .then(() => expect(this.interfacePage.getInterfaceNameInputField().isPresent()))
            .then(() => this.interfacePage.getInterfaceNameInputField().sendKeys(name))
            .then(() => this.interfacePage.getSaveInterfaceButton().click())
            .then(() => console.log("Interface created"));
    }

    public static bindInterfaceToSystem(systemName: string, interfaceName: string) {
        console.log(`Binding interface with name ${interfaceName} to system with name ${systemName}`);
        return this.gotoSpecificInterface(interfaceName)
            .then(() => Select2.searchFor(systemName, "s2id_interface-exposed-by"))
            .then(() => Select2.waitForDataAndSelect())
            .then(() => console.log("Interface bound to system"));;
    }

    public static waitForKendoGrid() {
        console.log("Waiting for kendo grid to be ready");
        return this.interfacePage.waitForKendoGrid();
    }

    public static gotoSpecificInterface(name: string) {
        console.log(`Navigating to interface with name ${name}`);
        return this.interfacePage.getPage()
            .then(() => this.waitForKendoGrid())
            .then(() => this.findSpecificInterfaceInNameColumn(name).click());
    }

    private static findSpecificInterfaceInNameColumn(name: string) {
        console.log(`Finding interface with name : ${name}`);
        return element(by.xpath('//*/tbody/*/td/a[text()="' + name + '" and @data-element-type="InterfaceName"]'));
    }
}
export = InterfaceCatalogHelper;