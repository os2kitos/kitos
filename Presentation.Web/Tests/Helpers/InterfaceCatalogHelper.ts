import InterfaceCatalogPage = require("../PageObjects/it-system/Interfaces/itSystemInterface.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import WaitTimers = require("../Utility/WaitTimers");
import Select2 = require("./Select2Helper");

class InterfaceCatalogHelper {
    private cssHelper = new CSSLocator();
    private waitUpTo = new WaitTimers();
    private interfacePage = new InterfaceCatalogPage();
    private EC = protractor.ExpectedConditions;

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
            .then(() => Select2.SearchFor(systemName, "s2id_interface-exposed-by"))
            .then(() => Select2.waitForDataAndSelect());
    }

    public waitForKendoGrid() {
        console.log("Waiting for kendo grid to be ready");
        return browser.wait(this.interfacePage.waitForKendoGrid(), this.waitUpTo.twentySeconds);
    }

    private gotoSpecificInterface(name : string) {
        console.log(`Navigating to interface with name ${name}`);
        return this.interfacePage.getPage()
            .then(() => this.findSpecificInterfaceInKendo(name).click());
    }

    private findSpecificInterfaceInKendo(name: string) {
        console.log(`Finding interface with name : ${name}`);
        return element(by.xpath('//*/tbody/*/td/a[text()="' + name + '"]/parent::*/parent::*//a'));
    }

}
export = InterfaceCatalogHelper;