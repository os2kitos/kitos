import InterfaceCatalogPage = require("../PageObjects/it-system/Interfaces/itSystemInterface.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import WaitTimers = require("../Utility/WaitTimers");
import Select2 = require("./Select2Helper");

class InterfaceCatalogHelper {
    private static cssHelper = new CSSLocator();
    private static waitUpTo = new WaitTimers();
    private static interfacePage = new InterfaceCatalogPage();

    public static createInterface(name: string) {
        console.log(`Creating interface with name ${name}`);
        return this.interfacePage.getPage()
            .then(() => this.interfacePage.waitForKendoGrid())
            .then(() => this.interfacePage.getCreateInterfaceButton().click())
            .then(() => expect(this.interfacePage.getInterfaceNameInputField().isPresent()))
            .then(() => this.interfacePage.getInterfaceNameInputField().sendKeys(name))
            .then(() => this.interfacePage.getSaveInterfaceButton().click());
    }

    public static bindInterfaceToSystem(systemName: string, interfaceName: string) {
        console.log(`Binding interface with name ${interfaceName} to system with name ${systemName}`);
        return this.gotoSpecificInterface(interfaceName)
            .then(() => element(this.cssHelper.byDataElementType("interfaceDetailsLink")).click())
            .then(() => Select2.SearchFor(systemName, "s2id_interface-exposed-by"))
            .then(() => Select2.waitForDataAndSelect());
    }

    public static waitForKendoGrid() {
        console.log("Waiting for kendo grid to be ready");
        return browser.wait(InterfaceCatalogHelper.interfacePage.waitForKendoGrid(), InterfaceCatalogHelper.waitUpTo.twentySeconds);
    }

    private static gotoSpecificInterface(name : string) {
        console.log(`Navigating to interface with name ${name}`);
        return this.interfacePage.getPage()
            .then(() => this.findSpecificInterfaceInKendo(name).click());
    }

    private static findSpecificInterfaceInKendo(name: string) {
        console.log(`Finding interface with name : ${name}`);
        return element(by.xpath('//*/tbody/*/td/a[text()="' + name + '"]/parent::*/parent::*//a'));
    }

}

export = InterfaceCatalogHelper;