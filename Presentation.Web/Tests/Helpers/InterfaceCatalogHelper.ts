import InterfaceCatalogPage = require("../PageObjects/it-system/Interfaces/itSystemInterface.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import WaitTimers = require("../Utility/WaitTimers");

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
            .then(() => this.select2SearchForInterface(systemName))
            .then(() => this.waitForSelect2DataAndSelect());
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

    public waitForSelect2DataAndSelect() {
        console.log(`waitForSelect2DataAndSelect`);
        return browser.wait(this.EC.visibilityOf(element(by.className("select2-result-label"))), 20000)
            .then(() => element(by.id("select2-drop")).element(by.className("select2-input")).sendKeys(protractor.Key.ENTER));
    }

    public select2SearchForInterface(name: string) {
        console.log(`select2SearchForMainSystem: ${name}`);
        return element(by.id("s2id_interface-exposed-by")).element(by.tagName('a')).click()
            .then(() => element(by.id("select2-drop")).element(by.className("select2-input")).click())
            .then(() => element(by.id("select2-drop")).element(by.className("select2-input")).sendKeys(name));
    }

}
export = InterfaceCatalogHelper;