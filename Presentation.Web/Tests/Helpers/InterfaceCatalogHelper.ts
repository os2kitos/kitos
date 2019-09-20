import InterfaceCatalogPage = require("../PageObjects/it-system/Interfaces/itSystemInterface.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import WaitTimers = require("../Utility/WaitTimers");
import TestFixture = require("../Utility/TestFixtureWrapper");

class InterfaceCatalogHelper {
    private static cssHelper = new CSSLocator();
    private static waitUpTo = new WaitTimers();
    private static testFixture = new TestFixture();
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
            .then(() => this.getExhibitsOfSelect2AndInputData(systemName))
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

    private static findSpecificInterfaceInKendo(name : string) {
        return element(by.xpath('//*/tbody/*/td/a[text()="' + name + '"]/parent::*/parent::*//a'));
    }
    
    private static getExhibitsOfSelect2AndInputData(search: string) {
        console.log(`Finding interface ${search}`);
        this.getSelect2ExhibitBox().click()
            .then(() => this.getSelect2ExhibitInputField().sendKeys(search));
    }

    private static  waitForSelect2DataAndSelect() {
        console.log(`waitForSelect2DataAndSelect`);
        return browser.wait(this.EC.visibilityOf(this.getSelect2ExhibitsResultsList()), 20000)
            .then(() => this.getSelect2ExhibitInputField().sendKeys(protractor.Key.ENTER))
            .then(() => browser.sleep(5000));
    }

    private static getSelect2ExhibitBox() {
        return element(by.id("s2id_interface-exposed-by")).element(by.tagName("a"));
    }

    private static getSelect2ExhibitInputField() {
        return element(by.id("s2id_interface-exposed-by")).element(by.tagName("input"));
    }

    private static getSelect2ExhibitsResultsList() {
        return element(by.xpath('//*/div[@class="select2-search"]/label[text()="Udstillet af"]/parent::*/parent::*/ul'));
    }

}

export = InterfaceCatalogHelper;