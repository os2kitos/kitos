import InterfaceCatalogPage = require("../PageObjects/it-system/Interfaces/itSystemInterface.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import WaitTimers = require("../Utility/WaitTimers");

class InterfaceCatalogHelper {
    private static cssHelper = new CSSLocator();
    private static waitUpTo = new WaitTimers();
    private static interfacePage = new InterfaceCatalogPage();
    private static EC = protractor.ExpectedConditions;

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
            .then(() => this.select2SearchForInterface(systemName))
            .then(() => this.waitForSelect2DataAndSelect());
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

    public static waitForSelect2DataAndSelect() {
        console.log(`waitForSelect2DataAndSelect`);
        return browser.wait(this.EC.visibilityOf(element(by.className("select2-result-label"))), 20000)
            .then(() => element(by.id("select2-drop")).element(by.className("select2-input")).sendKeys(protractor.Key.ENTER));
    }

    public static select2SearchForInterface(name: string) {
        console.log(`select2SearchForMainSystem: ${name}`);
        return element(by.id("s2id_interface-exposed-by")).element(by.tagName('a')).click()
            .then(() => element(by.id("select2-drop")).element(by.className("select2-input")).click())
            .then(() => element(by.id("select2-drop")).element(by.className("select2-input")).sendKeys(name));
    }

}
export = InterfaceCatalogHelper;